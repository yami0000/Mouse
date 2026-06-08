using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : DETECTION
{
    public int Num;
    public string targetSpawnId;
    public bool CanInteract;
    public bool AutoLoad;
   
    public override void Interact()
    {
        base.Interact();

        if (CanInteract)
            GameManager.Instance.LoadScene(Num, targetSpawnId);
     }

    
   
    public override void Event()
    {
        base.Event();
        if(AutoLoad)
            GameManager.Instance.LoadScene(Num, targetSpawnId);
    }
}
