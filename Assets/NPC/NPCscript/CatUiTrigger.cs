using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CatUiTrigger : MonoBehaviour
{
    [SerializeField] GameObject Prompt;

    private bool canOpenMenu;

    private void Start()
    {
        Prompt.SetActive(false);
    }


    private void Update()
    {
        if (canOpenMenu)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                openUI();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
            Prompt.SetActive(true);
            canOpenMenu = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
            Prompt.SetActive(false);
            canOpenMenu = false;
        }
    }

    private void openUI()
    {
        FindAnyObjectByType<UI_Cat>().OpenMenu();
    }
}
