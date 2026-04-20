using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Yarn.Unity;

public class NPCDialogue : DETECTION
{
    private DialogueRunner dialogueRunner;
    [SerializeField] private NPCDialogue npc;


    [Header("NPC Data")]
    public static NPCDialogue CurrentNPC { get; private set; }
    public string characterName = "";
    public string talkToNode = "";
   

    [Header("Optional")]
    public YarnProject scriptToLoad;
    private void Start()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();

        if (dialogueRunner != null)
        {

            dialogueRunner.onDialogueComplete.AddListener(OnDialogueEnded);
        }


        if (scriptToLoad != null)
        {
            DialogueRunner dialogueRunner = FindObjectOfType<DialogueRunner>();
             

        }
    }

    public override void Interact()
    {
        base.Interact();

        Debug.Log("interact");

        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
           return;
        else 
           Interaction();
    }

    private void Interaction()
    {

        if (dialogueRunner == null) return;

        CurrentNPC = npc;

        if (npc.characterName == "Grandma")
            GM.Instance.GameManager.isInteractGrandma = true;

        Debug.Log(CurrentNPC.characterName);

        dialogueRunner.StartDialogue(npc.talkToNode);
    }

    private void OnDialogueEnded()
    {
        if (CurrentNPC != null && CurrentNPC.characterName == "Grandma")
        {
            GM.Instance.GameManager.isInteractGrandma = false;  //   reset the flag here
        }

        CurrentNPC = null;
    }
}
