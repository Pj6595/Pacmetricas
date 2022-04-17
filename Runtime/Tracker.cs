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

    [System.Serializable]
    public struct Configuration
    { //Para la creacion del sistema de persistencia
        public PersistenceType persistenceType;
        public SerializationType serializationType;
        public int eventQueueSize;
        public string serverURL;
        public string requestContentType;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Tracker
    {
        private static Tracker instance = null;
        private Tracker() { }

        [SerializeField]
        public List<Configuration> persistenceConfiguration;

        private List<AbstractPersistence> persistences;
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
            persistences = new List<AbstractPersistence>();

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
                AbstractPersistence persistence;
                switch (configuration.persistenceType)
                {
                    case PersistenceType.FILE_PERSISTENCE:
                    default:
                        persistence = new FilePersistence(serializer, configuration.eventQueueSize, eventsEnabled);
                        break;
                    case PersistenceType.SERVER_PERSISTENCE:
                        persistence = new ServerPersistence(serializer, configuration.eventQueueSize,
                            configuration.serverURL, configuration.requestContentType, eventsEnabled);
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

        public void End()
        {
            telemetryActive = false;
            foreach (AbstractPersistence p in persistences)
            {
                p.Flush(); //Hace flush de todos los eventos que queden en la cola antes de cerrarse
                p.Stop();
            }
            foreach (Thread t in persistenceThreads)
            {
                Debug.Log("joineado");
                t.Join();
            };
            foreach (AbstractPersistence p in persistences) p.Stop();
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

        public void FlushAllEvents()
        {
            //Metodo para hacer flush de los eventos manualmente
            foreach (var persistenceElem in persistences)
            {
                persistenceElem.Flush();
            }
        }
    }
}