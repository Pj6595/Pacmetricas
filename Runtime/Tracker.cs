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
    public struct Configuration { public PersistenceType persistenceType; public SerializationType serializationType; }

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

        public static Tracker GetInstance()
        {
            if (instance == null)
                instance = new Tracker();
            
            return instance;
        }
        
        public void Init(List<Configuration> persistenceConfiguration)
        {
            activeTrackers = new List<ITrackerAsset>();
            persistences = new List<IPersistence>();

            foreach (var configuration in persistenceConfiguration)
            {
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

                IPersistence persistence;
                switch (configuration.persistenceType)
                {
                    case PersistenceType.FILE_PERSISTENCE:
                    default:
                        persistence = new FilePersistence(serializer);
                        break;
                    case PersistenceType.SERVER_PERSISTENCE:
                        persistence = new ServerPersistence(serializer);
                        break;
                }
                persistences.Add(persistence);

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
            foreach (IPersistence p in persistences) p.Stop();
            foreach (Thread t in persistenceThreads) {
                Debug.Log("joineado");
                t.Join(); }
        }

        public void TrackEvent(Event e)
        {
            if (telemetryActive)
            {
                //Luego será un for each con todas las persistencias que queramos
                foreach (var trackerAsset in activeTrackers)
                {
                    if (trackerAsset.Accept(e))
                    {
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