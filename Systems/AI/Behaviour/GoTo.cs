using UnityEngine;
using Location_Tool;


namespace AI_Behaviours
{
    public class GoTo : AI_Behaviour
    {
        Location location = null;
        Point destination = null;

        public GoTo(Agent _agent, BehaviourSettings _settings) : base(_agent, _settings)
        {
        }

        public override void OnStart()
        {
            Debug.Log("GoTo stared");
            location = LocationManager.GetLocation(settings.locationName);
            if(location != null)
            {
                if (settings.randomDestination)
                {
                    destination = location.GetRandomDestination(settings.destCatogory);
                }
                else
                {
                    destination = location.GetDestination(settings.destinationName);
                }

                if(destination == null)
                {
                    Debug.LogErrorFormat("destination {0} not found in location {1}", settings.destinationName, settings.locationName);
                    return;
                }
            }
            else
            {
                Debug.LogError("location " + settings.locationName + "  not found");
                return;
            }

            agent.NavAgent.isStopped = false;
            agent.NavAgent.destination = destination.position;
        }

        public override void Execute()
        {
        }

        public override void OnExit()
        {
            location = null;
            destination = null;
            agent.NavAgent.isStopped = true;
        }
    }
}
