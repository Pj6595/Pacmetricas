namespace Pacmetricas_G01{

	//public enum serializationFormat{ JSON, CSV } //esto es un apunte temporal

	public interface ISerializer{
		//public serializationFormat getFormat(); //esto es un apunte temporal
		public string Serialize(Event trackerEvent);
	}

	public class JSONSerializer: ISerializer{
		//public serializationFormat getFormat(){return serializationFormat.JSON;} //esto es un apunte temporal
		public string Serialize(Event trackerEvent){
			return trackerEvent.ToJSON();
		}
	}

	public class CSVSerializer: ISerializer{
		//public serializationFormat getFormat(){return serializationFormat.CSV;} //esto es un apunte temporal
		public string Serialize(Event trackerEvent){
			return trackerEvent.ToCSV();
		}
	}
}