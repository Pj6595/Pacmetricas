using System.IO;
using System.Collections;

namespace Pacmetricas_G01{
	
	public abstract class IPersistence{
		private Queue<Event> eventList = new Queue<Event>(20);
		public ISerializer serializer { get; }

		public abstract void SendEvent(Event trackerEvent);
		public abstract void Flush(); //asincrona
	}

	public class FilePersistence: IPersistence
	{
		public FilePersistence(ISerializer currSerializer){
			serializer = currSerializer;
		}

		public override void SendEvent(Event trackerEvent){
			eventList.Add(trackerEvent);
			//TO DO: hacer bien lmao
			if(eventList.Count >= 2) Flush();
		}
		
		public override void Flush() {
			string buffer = "";
			long gameSession;

			if(eventList.Empty()) return; //Si no hay eventos en la lista, no hace flush
			else gameSession = eventList.Peek().gameSession;

			while(!eventList.Empty()) {
				Event e = eventList.Dequeue();
				buffer += serializer.Serialize(e);
			}

			//Ruta en la que guardamos el archivo
			string path;
#if UNITY_EDITOR 
			path = Application.dataPath + "/Metricas"; 
#else
			path = Application.persistentDataPath + "/Metricas";
#endif

			if(!Directory.Exist(path)) Directory.CreateDirectory(path);

			path += "/gs_" + gameSession + serializer.GetSerializationFormat();

			FileStream fs;
			if(File.Exists(path))
				fs = File.Open(path, FileMode.Append);
			else 
				fs = File.Open(path, FileMode.Create);

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

		public override void SendEvent(Event trackerEvent){

		}
		public override void Flush(){
			
		}
	}
}