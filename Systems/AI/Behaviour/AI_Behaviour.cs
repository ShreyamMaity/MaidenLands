using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Location_Tool;


namespace AI_Behaviours
{
    public enum BehaviourType { Seek, GoTo }
}

namespace AI_Behaviours
{
    [System.Serializable]
    public class BehaviourSettings
    { 
        public Transform target;
        public float arriveRadius = 1.5f;

        [Header("Seek")]
        public float seekDelay = 0.8f;
        public float arriveThreshold = 3.0f;

        [Header("GoTo Location")]
        public bool randomDestination = true;
        public DestinationCategory destCatogory = DestinationCategory.Position;
        public string locationName = "";
        public string destinationName = "";

        // private
        [HideInInspector]
        public float time = 0.0f;

        [HideInInspector]
        public float currentTime;
    }


    [System.Serializable]
    public class AI_Behaviour
    {
        [HideInInspector]
        public Agent agent;
        [HideInInspector]
        public BehaviourSettings settings;

        public AI_Behaviour(Agent _agent, BehaviourSettings _settings)
        {
            agent = _agent;
            settings = _settings;
        }

        public virtual void OnStart() { }

        public virtual void Execute() { }

        public virtual void OnExit() { }
    }
}
