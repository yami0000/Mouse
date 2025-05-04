using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCstatemachine  
{
    public NPCstate currentState { get; private set; }

    public void Initialize(NPCstate _startState)//定义第一个动作以进入循环
    {
        currentState = _startState;
        currentState.Enter();
    }

    public void ChangeState(NPCstate _newState)
    {
        currentState.Exit();
        currentState = _newState;
        currentState.Enter();


    }
}
