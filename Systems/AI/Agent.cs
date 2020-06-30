using UnityEngine;
using AI_Behaviours;
using UnityEngine.AI;


public class Agent : HumanCharacter
{
    public AI_Behaviours.BehaviourType behaviourType = AI_Behaviours.BehaviourType.Seek;
    public BehaviourSettings behSettings = new BehaviourSettings();

    [HideInInspector]
    public NavMeshAgent NavAgent;

    [HideInInspector]
    public AI_Behaviour currentBehaviour;

    [HideInInspector]
    public AI_Behaviour previousBehaviour;

    public override void Start()
    {
        base.Start();
        NavAgent = GetComponent<NavMeshAgent>();

        // currentBehaviour = new Seek(this, behSettings);
        currentBehaviour = new GoTo(this, behSettings);
        currentBehaviour.OnStart();
    }

    void LateUpdate()
    {
        currentBehaviour.Execute();
    }

    public void SwitchBehaviour(AI_Behaviour newBehaviour)
    {
        if(currentBehaviour == null)
        {
            currentBehaviour = newBehaviour;
            currentBehaviour.OnStart();
            previousBehaviour = currentBehaviour;
            return;
        }

        if(currentBehaviour != newBehaviour)
        {
            previousBehaviour = currentBehaviour;
            currentBehaviour.OnExit();
            currentBehaviour = newBehaviour;
            currentBehaviour.OnStart();
        }
    }

    public override void OnAttack()
    {

    }

    public override void CreateAttack(HumanCharacter other)
    {

    }
}
