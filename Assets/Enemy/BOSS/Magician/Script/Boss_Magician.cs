using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Boss_Magician : Enemy
{
    private Transform player;
    public MagicianStartState startState{ get;private set; }   
    public MagicianPrepareState prepareState { get; private set; }
    public MagicianIdleState idleState { get; private set; }

    [HideInInspector] public float StateTimer_Magician;

    protected override void Awake()
    {
        base.Awake();

        startState = new MagicianStartState(this, stateMachine, "Start",this);
        prepareState = new MagicianPrepareState(this, stateMachine, "Prepare", this);
        idleState = new MagicianIdleState(this, stateMachine, "Idle", this);
    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(startState);

        player = PlayerManager.Instance.player.transform;
    }


    protected override void Update()
    {
        base.Update();
        StateTimer_Magician -= Time.deltaTime ;
    }

    public override void Die()
    {
        base.Die();
        //stateMachine.ChangeState(deathState);
    }
}
