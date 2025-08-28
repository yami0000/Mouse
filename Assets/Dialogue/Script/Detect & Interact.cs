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

    private void Start()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();    
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
        /*var allNPC = new List<NPCDialogue>(FindObjectsOfType<NPCDialogue>());
        var target = allNPC.Find(delegate (NPCDialogue npc)
        { return string.IsNullOrEmpty(npc.talkToNode) == false && (npc.transform.position - PlayerManager.player.transform.position).magnitude <= interactionRadius; });*/

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
}
