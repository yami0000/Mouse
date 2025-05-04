using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class NPC : Entity
{

    public NPCstatemachine stateMachine { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        stateMachine = new NPCstatemachine();
    }



    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();
    }
    public virtual void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();
}
