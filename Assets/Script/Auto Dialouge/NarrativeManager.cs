using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class NarrativeManager : MonoBehaviour
{
   public static NarrativeManager Instance { get; private set; }

    private DialogueRunner dialogueRunner;

    private Action currentAfterDialogueAction;
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

        dialogueRunner?.onDialogueComplete.AddListener(HandleDialogueFinished);
    }

    public void RequestDialogue(string nodeName, float t, Action onComplete = null)
    {
        if (dialogueRunner == null) return;

        currentAfterDialogueAction = onComplete;

        if (t == 0)
            dialogueRunner.StartDialogue(nodeName);
        else
            StartCoroutine(_StartDialogue(nodeName, t));
    }

    IEnumerator _StartDialogue(string nodeName,float t) 
    {
        yield return new WaitForSeconds(t);
        dialogueRunner.StartDialogue(nodeName);
       
    }

    private void HandleDialogueFinished()
    {
        
        if (currentAfterDialogueAction != null)
        {
            currentAfterDialogueAction.Invoke();

 
            currentAfterDialogueAction = null;
        }
    }
 
}
