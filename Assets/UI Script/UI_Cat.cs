using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Cat : MonoBehaviour
{
    [SerializeField] GameObject uiParent;

    bool isMenuOpen;

    private void Start()
    {
        uiParent.SetActive(false);
    }

    private void Update()
    {
        if (!isMenuOpen) return;

        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1))
        {
            CloseMenu();
        }
    }

    public void OpenMenu() 
    {
        if (isMenuOpen) return;

        isMenuOpen = true;  

        uiParent.SetActive(true);
        GM.Instance.GameManager.isUIOpened = true;
    }

    public void CloseMenu()
    {
        if (!isMenuOpen) return;

        isMenuOpen = false; 

        uiParent.SetActive(false);
        GM.Instance.GameManager.isUIOpened = false;
    }



}
