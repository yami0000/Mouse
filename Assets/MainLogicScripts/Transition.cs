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

    private void Awake()
    {
        if (AutoLoad)
        {
            AutoLoad = false;
            StartCoroutine(Hide());
        }
    }

    
    IEnumerator Hide() 
    {
       
       
        yield return new WaitForSeconds(0.3f);
        AutoLoad =true;
    }

    public override void Event()
    {
        base.Event();
        if(AutoLoad)
            GameManager.Instance.LoadScene(Num, targetSpawnId);
    }
}
