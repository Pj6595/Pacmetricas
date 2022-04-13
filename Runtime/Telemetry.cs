using UnityEngine;

namespace Pacmetricas_G01{

	public class Telemetry : MonoBehaviour{

		//Singleton pattern
		private static Telemetry instance = null;
		private Telemetry() {} //esto lo tiene asi guille en sus apuntes

		private FilePersistence persistence;
		private bool telemetryActive = false;

		//esto lo tiene asi guille en sus apuntes
		//no se si es necesario con el awake
		public static Telemetry GetInstance { 
			get {
				if(instance == null) {
					instance = new Singleton();
				}
				return instance;
			}
		}
		
		private void Awake()
		{
			if (instance != null)
			{
				Destroy(this.gameObject);
			}
			else
			{
				instance = this;
			}
		}
		
		private void Start(){
			Init();
		}

		public void Init(){
			telemetryActive = true;
			//TO DO: poner  los persistence en un array y el serializazer tambien (luego podremos modificar desde el editor cual queremos)
			persistence = new FilePersistence(new JSONSerializer());
		}

		public void End(){
			telemetryActive = false;
		}

		private void Update(){
			//Hacer flush periodico ¿?¿?¿
		}
		
		public void TrackEvent(Event e){
			if(telemetryActive){
				//Luego será un for each con todas las persistencias que queramos
				persistence.SendEvent(e);
			}
		}
	}
}