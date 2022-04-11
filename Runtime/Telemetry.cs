using UnityEngine;

namespace Pacmetricas_G01{

    public class Telemetry : MonoBehaviour{

        //Singleton pattern
        private static Telemetry _instance;
        private FilePersistance persistance;
        private bool telemetryActive = false;

        public static Telemetry GetInstance { get { return _instance; } }
        
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }
        
        private void Start(){
            Init();
        }

        public void Init(){
            telemetryActive = true;
            //TO DO: poner  los persistance en un array y el serializazer tambien (luego podremos modificar desde el editor cual queremos)
            persistance = new FilePersistance(new JSONSerializer());
        }

        public void End(){
            telemetryActive = true;
        }

        private void Update(){
            //Hacer flush periodico ¿?¿?¿
        }
        
        public void TrackEvent(Event e){
            if(telemetryActive){
                //Luego será un for each con todas las persistencias que queramos
                persistance.SendEvent(e);
            }
        }
    }
}