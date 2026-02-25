using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : DETECTION
{
    public int Num;
    public bool respawn;
    public bool CanInteract;
    public bool AutoLoad;
    public bool _walk;
    public float walktime;

    private void Start()
    {
        if (respawn)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                player.transform.position = transform.position;

            if (_walk)
            {
                Debug.Log("walk");
                Walk();
            }
        }
        
    }


    public override void Interact()
    {
        base.Interact();

        if (CanInteract)
        {
            SceneManager.LoadSceneAsync(Num);
        }
        

       
    }

    
    private void Walk() => StartCoroutine(walk() );
    
     IEnumerator walk() 
    {
        PlayerManager.Instance.player.isAutoControl = true;

        float t = walktime;

        

        while (t > 0)
        {
            t -= Time.deltaTime;

            PlayerManager.Instance.player.xInput = -1;

            

            yield return null;
        }
        PlayerManager.Instance.player.xInput = 0;

        PlayerManager.Instance.player.isAutoControl = false;


    }

    public override void Event()
    {
        base.Event();
        if(AutoLoad)
        SceneManager.LoadSceneAsync(Num);
    }
}
