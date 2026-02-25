using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCUiTrigger : DETECTION 
{ 
    public characterOrOther character;
    [SerializeField] GameObject Prompt;

    

    private bool canOpenMenu;

    private void Start()
    {
        if(Prompt == null)
            return;

        Prompt.SetActive(false);
    }


    private void Update()
    {
        if (canOpenMenu)
        {
            if (Input.GetKeyUp(KeyCode.E) && !GM.Instance.GameManager.isUIOpened)
            {
                openUI(character);
                 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
            canOpenMenu = true;

            if (Prompt == null)
                return;
            Prompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
            canOpenMenu = false;
            if (Prompt == null)
                return;
            Prompt.SetActive(false);
        }
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
