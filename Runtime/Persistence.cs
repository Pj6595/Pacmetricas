using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;

namespace Pacmetricas_G01{
	public interface IPersistence
    {
		public abstract void SendEvent(Event TrackerEvent);
		public abstract void Run();
		public abstract void Stop();
		public abstract void Flush();
    }
	
	public abstract class AbstractPersistence: IPersistence {
		protected int queueSize;
		protected Queue<Event> eventQueue;
		protected bool running = true;
		protected bool flushFlag = false;
		public EventTypes enabledEvents { get; set; }
		public ISerializer serializer { get; set; }
		public AbstractPersistence(ISerializer currSerializer, int queueSize, EventTypes enabledEvents = EventTypes.ALL_EVENTS) {
			serializer = currSerializer;
			this.queueSize = queueSize;
			eventQueue = new Queue<Event>(queueSize);
			this.enabledEvents = enabledEvents;
		}
		public void SendEvent(Event trackerEvent)
        {
			if((Event.EventIDs[trackerEvent.type] & enabledEvents) != 0)	// yo recojo este evento??
			{
				lock (eventQueue)
				{ //Se bloquea la cola para incluir un evento 
					eventQueue.Enqueue(trackerEvent);
				}
			}
		}
		public void Run()
		{
			while (running)
			{
				if (eventQueue.Count == queueSize || flushFlag)
				{
					//La cola de eventos se vacia siempre que esta llena
					_Flush();
					flushFlag = false;
				}
			}
		}
		public void Stop()
        {
			running = false;
			_Flush();
        }
		public void Flush()
		{
			flushFlag = true;
		}

		protected abstract void _Flush();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public class FilePersistence: AbstractPersistence
	{
		private string path;

		public FilePersistence(ISerializer currSerializer, int queueSize, EventTypes enabledEvents = EventTypes.ALL_EVENTS) :
			base(currSerializer, queueSize, enabledEvents)
		{
#if UNITY_EDITOR
			path = Application.dataPath + "/Metricas";
#else
			path = Application.persistentDataPath + "/Metricas";
#endif
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
		}
		
		protected override void _Flush() {
			lock (eventQueue) { //Se bloquea la cola para hacer flush sin que se haga enqueue de nuevos eventos
				long gameSession;
				long timeStamp;
				if(eventQueue.Count == 0) return; //Si no hay eventos en la lista, no hace flush
				else{ 
					gameSession = eventQueue.Peek().gameSession;
					timeStamp = eventQueue.Peek().timeStamp;
				}

				while(eventQueue.Count != 0) {
					Event e = eventQueue.Dequeue();
					serializer.SerializeEvent(e);
				}

				string buffer = serializer.GetFullSerialization();
				string filePath = path + "/ts_"+ timeStamp + "gs_" + gameSession + serializer.GetSerializationFormat();

				FileStream fs;
				if(File.Exists(filePath))
					fs = File.Open(filePath, FileMode.Truncate);
				else 
					fs = File.Open(filePath, FileMode.Create);
					
				//Escritura en archivo
				StreamWriter sw = new StreamWriter(fs,System.Text.Encoding.UTF8);
				sw.WriteLine(buffer);
				sw.Close();
			}
		}
	}

	public class ServerPersistence: AbstractPersistence{
		private string serverURL;
		private string contentType;
		private List<RequestHeader> header = null;

		public ServerPersistence(ISerializer currSerializer, int queueSize, 
			string url, string contentType, List<RequestHeader> header, EventTypes enabledEvents = EventTypes.ALL_EVENTS) :
			base(currSerializer, queueSize, enabledEvents)
		{
			serverURL = url;
			this.contentType = contentType;
			this.header = header;
		}

		protected override void _Flush(){
			string buffer = "";
			lock (eventQueue)
			{
				long gameSession;

				if (eventQueue.Count == 0) return; //Si no hay eventos en la lista, no hace flush
				else gameSession = eventQueue.Peek().gameSession;

				while (eventQueue.Count != 0)
				{
					Event e = eventQueue.Dequeue();
					serializer.SerializeEvent(e);
				}
				buffer = serializer.GetFullSerialization();
				serializer.FlushSerialization();
			}

			var request = WebRequest.CreateHttp(serverURL);
			if (header != null)
				foreach (RequestHeader h in header) request.Headers.Add(h.key + ": " + h.value);
			request.Method = "POST";
			request.ContentType = contentType;
			var requestBuffer = Encoding.UTF8.GetBytes(buffer);
			request.ContentLength = requestBuffer.Length;
			request.GetRequestStream().Write(requestBuffer, 0, buffer.Length);
		}
	}
}