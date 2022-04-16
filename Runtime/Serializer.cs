namespace Pacmetricas_G01 
{	
	public interface ISerializer{
		public string GetSerializationFormat(); //Devuelve la extension de archivo
		public string SerializeEvent(Event trackerEvent);
		public string GetFullSerialization();
	}

	public class JSONSerializer: ISerializer {
		private string serialization =""; 
		public string GetSerializationFormat() {return ".json";}
		public string SerializeEvent(Event trackerEvent) {
			string buffer = trackerEvent.ToJSON();
			serialization += buffer + ",";
			return buffer;
		}

		public string GetFullSerialization() {
			return "[" + serialization.Remove(serialization.Length - 1) + "]";
		}
	}

	public class CSVSerializer: ISerializer {
		private string serialization = "TipoEvento,TimeStamp,GameSession,Comando,Valor\n";
		public string GetSerializationFormat() {return ".csv";} 
		public string SerializeEvent(Event trackerEvent) {
			string buffer = trackerEvent.ToCSV();
			serialization += buffer;
			return buffer;
		}

		public string GetFullSerialization() {
			return serialization;
		}
	}
}