using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Enemy_Bee : Enemy
{
    public BeeFlyState flyState {  get; private set; }
    public BeeDieState deathState { get; private set; }
    public BeeBattleState battleState { get; private set; }
    public BeeAttackState attackState { get; private set; }

   

    [SerializeField] public EnemyAmmo ammo;
    protected override void Awake()
    {
        base.Awake();
 

        flyState = new BeeFlyState(this, stateMachine, "Fly", this);
        deathState = new BeeDieState(this, stateMachine, "Death", this);
        battleState = new BeeBattleState(this, stateMachine, "Fly", this);
        attackState = new BeeAttackState(this, stateMachine, "Attack", this);
    }

 
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(flyState);
    }

    protected override void Update()
    {
        base.Update();
    }
    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deathState);
    }


}
