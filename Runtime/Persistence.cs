namespace Pacmetricas_G01{
	
	public abstract class IPersistence{
		public abstract void SendEvent(Event trackerEvent);
		public abstract void Flush(); //asincrona
	}

	public abstract class Persistence: IPersistence
	{
		private Queue<Event> eventList = new Queue<Event>(20);
		public ISerializer serializer {get;}
	}

	public class FilePersistence: Persistence
	{
		public FilePersistence(ISerializer currSerializer){
			serializer = currSerializer;
		}

		public override void SendEvent(Event trackerEvent){
			eventList.Add(trackerEvent);
		}
		
		public override void Flush() {
			string buffer = "";
			while(!eventList.Empty()) {
				Event e = eventList.pop();
				buffer += serializer.Serialize(e);
			}
			
		}
	}

	public class ServerPersistence: Persistence{

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