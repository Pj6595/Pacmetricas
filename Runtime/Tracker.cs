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

    [System.Flags]
    public enum TrackerAssetType
    {
        _NOTHING = 0,
        INIT_GAME = 1 << 0,
        END_GAME = 1 << 1,
        MENU_PASSED = 1 << 2,
        FIRST_PHRASE = 1 << 3,
        CORRECT_DIR = 1 << 4,
        INIT_RUN = 1 << 5,
        PLAYER_DEAD = 1 << 6,
        TRY_PHRASE_MENU = 1 << 7,
        TRY_PHRASE_TAXI = 1 << 8,
        TRY_PHRASE_BLACKOUT = 1 << 9,
        VOLUME_MIC = 1 << 10,
        BLACKOUT_INTENSITY = 1 << 11,
        _ALL = ~0
    }

    [System.Serializable]
    public struct Configuration { //Para la creacion del sistema de persistencia
        public PersistenceType persistenceType;
        public SerializationType serializationType;
        public TrackerAssetType enabledEvents;
        public int eventQueueSize;
        public string serverURL;
    }

    public class Tracker
    {
        private static Tracker instance = null;
        private Tracker() { }

        [SerializeField]
        public List<Configuration> persistenceConfiguration;

        private List<IPersistence> persistences;
        private bool telemetryActive = false;
        private List<ITrackerAsset> activeTrackers;

        List<Thread> persistenceThreads = new List<Thread>();

        public static Tracker GetInstance() {
            if (instance == null)
                instance = new Tracker();
            
            return instance;
        }
        
        public void Init(List<Configuration> persistenceConfiguration)
        {
            activeTrackers = new List<ITrackerAsset>();
            persistences = new List<IPersistence>();

            //Creacion de distintas persistencias a partir de la lista de configuraciones
            foreach (var configuration in persistenceConfiguration)
            {
                Debug.Log((int)(configuration.enabledEvents));

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
                        persistence = new FilePersistence(serializer, configuration.eventQueueSize);
                        break;
                    case PersistenceType.SERVER_PERSISTENCE:
                        persistence = new ServerPersistence(serializer, configuration.eventQueueSize, configuration.serverURL);
                        break;
                }
                persistences.Add(persistence);

                //Se crea un hilo para cada persistencia
                Thread persistenceThread = new Thread(persistence.Run);
                persistenceThreads.Add(persistenceThread);
                persistenceThread.Start();
            }
            
            activeTrackers.Add(new TrackerAsset());
            telemetryActive = true;
        }

        public void End()
        {
            telemetryActive = false;
            foreach (IPersistence p in persistences) {
                p.Flush(); //Hace flush de todos los eventos que queden en la cola antes de cerrarse
                p.Stop();
            }
            foreach (Thread t in persistenceThreads) {
                Debug.Log("joineado");
                t.Join();
            };
            foreach (IPersistence p in persistences) p.Stop();
            foreach (Thread t in persistenceThreads) t.Join();
        }

        public void TrackEvent(Event e)
        {
            //Se pasa el evento e por todos los tracker asset
            if (telemetryActive) {
                foreach (var trackerAsset in activeTrackers)
                {
                    //Si alguno lo acepta se envia a todas las persistencias
                    if (trackerAsset.Accept(e)) { 
                        foreach (var persistenceElem in persistences)
                        {
                            persistenceElem.SendEvent(e);
                        }
                        return;
                    }
                }
            }
        }
    }
}