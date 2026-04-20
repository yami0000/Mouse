using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCUiTrigger : DETECTION 
{ 
    public characterOrOther character;

    public override void Interact()
    {
        base.Interact();
        openUI(character);
    }

    private void openUI(characterOrOther A)
    {
        UI_NPC[] allNPCs = FindObjectsByType<UI_NPC>(FindObjectsSortMode.None);

        foreach (var npc in allNPCs)
        {
            if (npc.Who == A)
            {
                npc.OpenMenu();
                return; 
            }
        }
    }
}
