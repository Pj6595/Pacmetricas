namespace Pacmetricas_G01{
	
	public interface ISerializer{
		public string GetSerializationFormat();
		public void SerializeEvent(Event trackerEvent);
		public string GetSerialization();
	}

	public class JSONSerializer: ISerializer {

		private string serialization ="["; 
		public string GetSerializationFormat() {return ".json";}
		public void SerializeEvent(Event trackerEvent){
			serialization += trackerEvent.ToJSON() + ",";
		}

		public string GetSerialization(){
			return serialization.Remove(serialization.Length - 1) + "]";
		}
	}

	public class CSVSerializer: ISerializer {
		private string serialization = "TipoEvento,TimeStamp,GameSession,Comando,Valor\n";
		public string GetSerializationFormat() {return ".csv";} 
		public void SerializeEvent(Event trackerEvent){
			serialization += trackerEvent.ToCSV();
		}

		public string GetSerialization(){
			return serialization;
		}
	}
}