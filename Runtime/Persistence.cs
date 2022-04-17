using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;

namespace Pacmetricas_G01{
	
	public abstract class IPersistence {
		protected int queueSize;
		protected Queue<Event> eventQueue;
		protected bool running = true;
		public EventTypes enabledEvents { get; set; }
		public ISerializer serializer { get; set; }
		public IPersistence(ISerializer currSerializer, int queueSize) {
			serializer = currSerializer;
			this.queueSize = queueSize;
			eventQueue = new Queue<Event>(queueSize);
		}
		public void SendEvent(Event trackerEvent)
        {
			if(EventIDs[trackerEvent.type] & enabledEvents)	// yo recojo este evento??
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
				if (eventQueue.Count == queueSize)
				{
					//La cola de eventos se vacia siempre que esta llena
					Flush();
				}
			}
		}
		public void Stop()
        {
			running = false;
        }
		public abstract void Flush(); //Asincrona
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public class FilePersistence: IPersistence
	{
		private string path;

		public FilePersistence(ISerializer currSerializer, int queueSize, EventTypes enabledEvents = EventTypes._All) :
			base(currSerializer, queueSize)
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
		
		public override void Flush() {
			lock (eventQueue) { //Se bloquea la cola para hacer flush sin que se haga enqueue de nuevos eventos
				long gameSession;

				if(eventQueue.Count == 0) return; //Si no hay eventos en la lista, no hace flush
				else gameSession = eventQueue.Peek().gameSession;

				while(eventQueue.Count != 0) {
					Event e = eventQueue.Dequeue();
					serializer.SerializeEvent(e);
				}

				string buffer = serializer.GetFullSerialization();
				string filePath = path + "/gs_" + gameSession + serializer.GetSerializationFormat();

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

	public class ServerPersistence: IPersistence{
		private string serverURL;
		public ServerPersistence(ISerializer currSerializer, int queueSize, string url, EventTypes enabledEvents = EventTypes._All) :
			base(currSerializer, queueSize)
		{
			serverURL = url;
		}

		public override void Flush(){
            lock (eventQueue)
            {
				long gameSession;

				if (eventQueue.Count == 0) return; //Si no hay eventos en la lista, no hace flush
				else gameSession = eventQueue.Peek().gameSession;

				while (eventQueue.Count != 0)
				{
					Event e = eventQueue.Dequeue();
					string content = serializer.SerializeEvent(e);
					var request = WebRequest.CreateHttp(serverURL);
					request.Method = "POST";
					request.ContentType = "application/json";
					var buffer = Encoding.UTF8.GetBytes(content);
					request.ContentLength = buffer.Length;
					request.GetRequestStream().Write(buffer, 0, buffer.Length);
				}
			}
		}
	}
}