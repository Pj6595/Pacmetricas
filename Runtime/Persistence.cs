using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Pacmetricas_G01{
	
	public abstract class IPersistence {
		protected int queueSize;
		protected Queue<Event> eventQueue = new Queue<Event>(queueSize);
		protected bool running = true;

		public ISerializer serializer { get; set; }
		public abstract void SendEvent(Event trackerEvent);
		public abstract void Run();
		public void Stop()
        {
			running = false;
        }
		public abstract void Flush(); //Asincrona
	}

	public class FilePersistence: IPersistence
	{
		private string path;

		public FilePersistence(ISerializer currSerializer, int queueSize){
			serializer = currSerializer;
			this.queueSize = queueSize
#if UNITY_EDITOR
			path = Application.dataPath + "/Metricas";
#else
			path = Application.persistentDataPath + "/Metricas";
#endif
			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
		}

		public override void SendEvent(Event trackerEvent) {
            lock (eventQueue) { //Se bloquea la cola para incluir un evento 
				eventQueue.Enqueue(trackerEvent);
			}
		}

		public override void Run() {
            while (running) {
				if (eventQueue.Count == queueSize) {
					//La cola de eventos se vacia siempre que esta llena
					Flush();
				}
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

				string buffer = serializer.GetSerialization();
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

		//Server ??? 
		public ServerPersistence(ISerializer currSerializer, int queueSize){
			serializer = currSerializer;
			this.queueSize = queueSize;
		}

        public override void Run()
        {
            throw new System.NotImplementedException();
        }

        public override void SendEvent(Event trackerEvent){

		}
		public override void Flush(){
			
		}
	}
}