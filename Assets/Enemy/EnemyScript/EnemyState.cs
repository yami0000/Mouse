using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState  
{
  protected EnemyStateMachine stateMachine;
  protected Enemy enemyBase;
  protected Rigidbody2D rb;

    private string animBoolName;
    public string AnimBoolName => animBoolName;

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
    
    public virtual void Update() 
    {
        stateTimer -= Time.deltaTime;
        stateTimer_Bee -= Time.deltaTime;
        stateTimer_SelfDes -= Time.deltaTime;
        stateTimer_Laser -= Time.deltaTime;



    }

    #region Move animation
    // Opt in helpers for states that move and stop within the one state, and want the animation
    // to follow. Pair every velocity change with MoveAt or StandStill and the two can never
    // drift apart, which is what leaves an enemy sliding along in a walk cycle while stopped.

    /// <summary>True while this state is holding the move animation rather than the idle one.</summary>
    protected bool isMoving;

    /// <summary>Walks at _speed the way the enemy faces, and holds the move animation.</summary>
    protected void MoveAt(float _speed)
    {
        enemyBase.Setvelocity(_speed * enemyBase.facingDir, rb.velocity.y);
        SetMoving(true);
    }

    /// <summary>Stops dead and holds the idle animation, without leaving the state.</summary>
    protected void StandStill()
    {
        enemyBase.zerovelocity();
        SetMoving(false);
    }

    protected void SetMoving(bool _moving)
    {
        // Only on a change, so SetBool is not hammered every frame.
        if (isMoving == _moving)
            return;

        isMoving = _moving;

        enemyBase.anim.SetBool(enemyBase.battleMoveAnimBool, _moving);
        enemyBase.anim.SetBool(enemyBase.battleIdleAnimBool, !_moving);
    }

    /// <summary>
    /// Clears both bools and tells the death state which one was actually showing. Call it from
    /// Exit in any state that uses MoveAt or StandStill: the base Exit only knows about the one
    /// bool the state was built with, so the other would stay raised into the next state.
    /// </summary>
    protected void ClearMoveAnimBools()
    {
        enemyBase.anim.SetBool(enemyBase.battleMoveAnimBool, false);
        enemyBase.anim.SetBool(enemyBase.battleIdleAnimBool, false);

        enemyBase.AssignLastBoolName(isMoving ? enemyBase.battleMoveAnimBool : enemyBase.battleIdleAnimBool);
    }
    #endregion

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
