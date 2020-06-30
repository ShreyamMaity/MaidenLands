using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI_Behaviours
{
    public class Seek : AI_Behaviour
    {
        public Seek(Agent _agent, BehaviourSettings _settings) : base(_agent, _settings)
        {
        }

        public override void OnStart()
        {
            Debug.Log("Seek stared");
            agent.NavAgent.isStopped = false;
        }

        public override void Execute()
        {
            // seek target with delay
            float distance = Vector3.Distance(agent.transform.position, settings.target.position);
            if (distance > settings.arriveRadius)
            {
                settings.currentTime += Time.deltaTime;
                if (settings.currentTime > settings.seekDelay)
                {
                    agent.NavAgent.destination = settings.target.position;
                }
            }
            else
            {
                settings.currentTime = 0.0f;
            }
        }

        public override void OnExit()
        {
            agent.NavAgent.isStopped = true;
        }
    }
}
