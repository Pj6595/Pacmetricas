namespace Pacmetricas_G01{

	public interface ISerializer{
		public string Serialize(Event trackerEvent);
	}

	public class JSONSerializer: ISerializer{
		public string Serialize(Event trackerEvent){
			return trackerEvent.ToJSON();
		}
	}

	public class CSVSerializer: ISerializer{
		public string Serialize(Event trackerEvent){
			return trackerEvent.ToCSV();
		}
	}
}