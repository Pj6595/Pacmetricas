using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Pacmetricas_G01{
	
	public abstract class IPersistence{
		protected Queue<Event> eventList = new Queue<Event>(20);
		protected bool running = true;

		public ISerializer serializer { get; set; }

		public abstract void SendEvent(Event trackerEvent);

		public abstract void Run();

		public void Stop()
        {
			running = false;
        }

		public abstract void Flush(); //asincrona
	}

	public class FilePersistence: IPersistence
	{
		private string path;

		public FilePersistence(ISerializer currSerializer){
			serializer = currSerializer;
#if UNITY_EDITOR
			path = Application.dataPath + "/Metricas";
#else
			path = Application.persistentDataPath + "/Metricas";
#endif
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
		}

		public override void SendEvent(Event trackerEvent){
            lock (eventList)
            {
				eventList.Enqueue(trackerEvent);
			}
		}

		public override void Run()
        {
            while (running){
				lock (eventList)
				{
					//TO DO: hacer bien lmao
					if (eventList.Count >= 1) Flush();
				}
			}
		}
		
		public override void Flush() {
			long gameSession;

			if(eventList.Count == 0) return; //Si no hay eventos en la lista, no hace flush
			else gameSession = eventList.Peek().gameSession;

			while(eventList.Count != 0) {
				Event e = eventList.Dequeue();
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

	public class ServerPersistence: IPersistence{

		//Server ??? 
		public ServerPersistence(ISerializer currSerializer){
			serializer = currSerializer;
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