using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState  
{
  protected EnemyStateMachine stateMachine;
  protected Enemy enemyBase;
  protected Rigidbody2D rb;

    private string animBoolName;

    protected bool triggerCalled;
    protected bool triggerCalled_Bee;
    protected bool ReadyToAttack;
    protected bool ReadyToAttack_Bee;
    protected bool triggerCalled_Medic ;
    protected bool ReadyToAttack_Medic ;

    protected float stateTimer;
    protected float stateTimer_Bee;
    protected float stateTimer_SelfDes;
    protected float stateTimer_Laser;

    public EnemyState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
    {
        this.stateMachine = _stateMachine;
        this.enemyBase = _enemyBase;
        this.animBoolName = _animBoolName;
    }

    public virtual void Update() 
    {
        stateTimer -= Time.deltaTime;
        stateTimer_Bee -= Time.deltaTime;
        stateTimer_SelfDes -= Time.deltaTime;
        stateTimer_Laser -= Time.deltaTime;



    }

    public virtual void Enter()
    { 
    triggerCalled = false;
    ReadyToAttack = false;
    triggerCalled_Bee = false;
    ReadyToAttack_Bee = false;
    triggerCalled_Medic = false;
    ReadyToAttack_Medic = false ;


                rb = enemyBase.rb;
    enemyBase.anim.SetBool(animBoolName,true);
    
    }

    public virtual void Exit() 
    {
    enemyBase.anim.SetBool(animBoolName,false);
    enemyBase.AssignLastBoolName(animBoolName);
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled=true;
    }

    public virtual void AttackPreparetionTrigger()
    {
        ReadyToAttack = true;
    }

    public virtual void AnimationFinishTrigger_Bee()
    {
        triggerCalled_Bee = true;
    }

    public virtual void AttackPreparetionTrigger_Bee()
    {
        ReadyToAttack_Bee = true;
    }

    public virtual void AnimationFinishTrigger_Medic()
    {
        triggerCalled_Medic = true;
    }

    public virtual void AttackPreparetionTrigger_Medic()
    {
        ReadyToAttack_Medic = true;
    }
}
