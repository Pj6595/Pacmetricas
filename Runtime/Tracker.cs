using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Pacmetricas_G01
{
    [System.Serializable]
    public enum PersistenceType
    {
        FILE_PERSISTENCE, SERVER_PERSISTENCE
    }
    [System.Serializable]
    public enum SerializationType
    {
        JSON_SERIALIZATION, CSV_SERIALIZATION
    }

    //Para la creacion del sistema de persistencia
    [System.Serializable]
    public struct Configuration
    { 
        public PersistenceType persistenceType;
        public SerializationType serializationType;
        public int eventQueueSize;
        public string serverURL;
        public string requestContentType;
        public List<RequestHeader> requestHeader;
    }

    [System.Serializable]
    public struct RequestHeader
    {
        public string key;
        public string value;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Tracker
    {
        private static Tracker instance = null;
        private Tracker() { }

        [SerializeField]
        public List<Configuration> persistenceConfiguration;

        private List<IPersistence> persistences;
        private bool telemetryActive = false;
        List<Thread> persistenceThreads = new List<Thread>();

        public static Tracker GetInstance()
        {
            if (instance == null)
                instance = new Tracker();

            return instance;
        }

        public void Init(List<Configuration> persistenceConfiguration, EventTypes eventsEnabled)
        {
            persistences = new List<IPersistence>();

            //Creacion de distintas persistencias a partir de la lista de configuraciones
            foreach (var configuration in persistenceConfiguration)
            {
                //Serializador para la persistencia
                ISerializer serializer;
                switch (configuration.serializationType)
                {
                    case SerializationType.JSON_SERIALIZATION:
                    default:
                        serializer = new JSONSerializer();
                        break;
                    case SerializationType.CSV_SERIALIZATION:
                        serializer = new CSVSerializer();
                        break;
                }

                //Persistencia con su serializador y tamanho de cola de eventos
                IPersistence persistence;
                switch (configuration.persistenceType)
                {
                    case PersistenceType.FILE_PERSISTENCE:
                    default:
                        persistence = new FilePersistence(serializer, configuration.eventQueueSize, eventsEnabled);
                        break;
                    case PersistenceType.SERVER_PERSISTENCE:
                        persistence = new ServerPersistence(serializer, configuration.eventQueueSize,
                            configuration.serverURL, configuration.requestContentType, configuration.requestHeader, eventsEnabled);
                        break;
                }
                persistences.Add(persistence);

                //Se crea un hilo para cada persistencia
                Thread persistenceThread = new Thread(persistence.Run);
                persistenceThreads.Add(persistenceThread);
                persistenceThread.Start();
            }

            telemetryActive = true;
        }

        //Cerrar el tracker
        public void End()
        {
            telemetryActive = false;
            foreach (IPersistence p in persistences)
            {
                p.Stop();
            }
            foreach (Thread t in persistenceThreads){
                t.Join();
            };
            foreach (IPersistence p in persistences) p.Stop();
            foreach (Thread t in persistenceThreads) t.Join();
        }

        public void TrackEvent(Event e)
        {
            //Se pasa el evento e por todos los tracker asset
            if (telemetryActive)
            {
                foreach (var persistenceElem in persistences)
                {
                    persistenceElem.SendEvent(e);
                }
            }

        }
        
        //Metodo para hacer flush de los eventos manualmente
        public void FlushAllEvents()
        {
            foreach (var persistenceElem in persistences)
            {
                persistenceElem.Flush();
            }
        }
    }
}