using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Samples;

public class Detect_Interact : MonoBehaviour
{   
    private DialogueRunner dialogueRunner;
    [SerializeField]private NPCDialogue npc;

    private bool isInteractable;

    public static NPCDialogue CurrentNPC { get; private set; }


    private void Start()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();

        if (dialogueRunner != null)
        {
             
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueEnded);
        }
    }
    private void Update()
    {
        if (isInteractable)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
                    return;
                else
                    Interact();
            }
        }
    }

    private void Interact() 
    {

        if (dialogueRunner == null) return;

        CurrentNPC = npc;

        if (npc.characterName == "Grandma")
            GM.Instance.GameManager.isInteractGrandma = true;

        Debug.Log(GM.Instance.GameManager.isInteractGrandma);
        Debug.Log(CurrentNPC.characterName);

        dialogueRunner.StartDialogue(npc.talkToNode);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
            isInteractable = true;
            
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
            isInteractable = false;
            
        }
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
