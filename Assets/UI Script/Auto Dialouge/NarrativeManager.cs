using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class NarrativeManager : MonoBehaviour
{
   public static NarrativeManager Instance { get; private set; }

    private DialogueRunner dialogueRunner;
    private void Awake()
    {
        // Setup the Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();
    }

    public void RequestDialogue(string nodeName,float t)
    {

        if (dialogueRunner == null) return;

        if (t == 0)
            dialogueRunner.StartDialogue(nodeName);
        else
            StartCoroutine( _StartDialogue(nodeName,t));


    }

    IEnumerator _StartDialogue(string nodeName,float t) 
    {
        yield return new WaitForSeconds(t);
        dialogueRunner.StartDialogue(nodeName);
       
    }
}
