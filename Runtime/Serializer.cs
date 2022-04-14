namespace Pacmetricas_G01{

	public interface ISerializer{
		public string GetSerializationFormat();
		public string Serialize(Event trackerEvent);
	}

	public class JSONSerializer: ISerializer {
		public string GetSerializationFormat() {return ".json";}
		public string Serialize(Event trackerEvent){
			return trackerEvent.ToJSON();
		}
	}

	public class CSVSerializer: ISerializer {
		public string GetSerializationFormat() {return ".csv";} 
		public string Serialize(Event trackerEvent){
			return trackerEvent.ToCSV();
		}
	}
}