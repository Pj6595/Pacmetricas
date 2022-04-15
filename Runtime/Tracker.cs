using System.Collections.Generic;
using System;
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

    public class Tracker : MonoBehaviour
    {
        private static Tracker instance = null;
        private Tracker() { }

        [SerializeField]
        public List<Configuration> persistenceConfiguration;

        private List<IPersistence> persistences;
        private bool telemetryActive = false;
        private List<ITrackerAsset> activeTrackers;
        public static Tracker GetInstance()
        {
            return instance;
        }

        private void Awake()
        {
            if (instance != null)
                Destroy(this.gameObject);
            else
            {
                instance = this;
                Init();
            }
        }

        public void Init()
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
            }
            telemetryActive = true;
            activeTrackers.Add(new TrackerAsset());

        }

        public void End()
        {
            telemetryActive = false;
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