using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    [SerializeField] GameObject uiParent;

    private bool isMenuOpen;
    private bool canOpenMenu = true;

    private void Start()
    {
        uiParent.SetActive(false);
    }

    private void Update()
    {
        

        if (canOpenMenu) 
        if (Input.GetKeyUp(KeyCode.I)  )
        {
           OpenMenu();
           canOpenMenu = false;
                Debug.Log("open!");
        }


        if (!isMenuOpen) return;
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1))
        {
            CloseMenu();
            canOpenMenu = true;
        }
    }

    public void OpenMenu()
    {
        if (isMenuOpen) return;

        isMenuOpen = true;

        uiParent.SetActive(true);
    }

    public void CloseMenu()
    {
        if (!isMenuOpen) return;

        isMenuOpen = false;

        uiParent.SetActive(false);
    }

    private void openUI()
    {
        FindAnyObjectByType<UI_Inventory>().OpenMenu();
    }
}
