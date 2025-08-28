using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class NPCDialogue : MonoBehaviour
{
    public string characterName = "";
    public string talkToNode = "";
   

    [Header("Optional")]
    public YarnProject scriptToLoad;
    private void Start()
    {
        if (scriptToLoad != null)
        {
            DialogueRunner dialogueRunner = FindObjectOfType<DialogueRunner>();
             

        }
    }

   

}
