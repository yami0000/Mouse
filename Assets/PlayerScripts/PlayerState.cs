using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected Player player;

    protected Rigidbody2D rb;

    protected float xInput;
    protected float yInput;
    private string animBoolName;

    protected float stateTimer;
    protected bool triggerCalled;
    protected bool Keyframe;//
  

    public PlayerState(Player _player,PlayerStateMachine _stateMachine,string _animBoolName)
    {
        this.player = _player;
        this.stateMachine = _stateMachine;
        this.animBoolName = _animBoolName;


    }
    public virtual void Enter()
    {
        player.anim.SetBool(animBoolName, true);
        rb = player.rb;
        triggerCalled = false;
        Keyframe = false;   
    
    }
    public virtual void Update() {

        stateTimer -= Time.deltaTime;
      
        xInput= Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

       
        
    }
    public virtual void Exit() {

     
        player.anim.SetBool(animBoolName, false);
    }

    public virtual void AnimationFinishTrigger()
    {
       triggerCalled = true;

    }

    public virtual void KeyframeTrigger() 
    {
       Keyframe = true;
    }

}



