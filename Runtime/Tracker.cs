using UnityEngine;
using System.Collections.Generic;

namespace Pacmetricas_G01
{

    public class Tracker : MonoBehaviour
    {
        private static Tracker instance = null;
        private Tracker() { }
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
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
            }
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            telemetryActive = true;
            activeTrackers.Add(new TrackerAsset());
            //TO DO: poner  los persistence en un array y el serializazer tambien (luego podremos modificar desde el editor cual queremomos
            persistences.Add(new FilePersistence(new JSONSerializer()));
        }

        public void End()
        {
            telemetryActive = false;
        }

        private void Update()
        {
            //Hacer flush periodico ¿?¿?¿
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