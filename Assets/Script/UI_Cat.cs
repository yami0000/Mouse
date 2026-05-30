using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum characterOrOther
{
Cat,
Dog,
Mary,
Station,
Vendor




}
public class UI_NPC : MonoBehaviour
{
    public characterOrOther Who;
    public FastTravelAni FT;
    public bool isItInherited;

    [SerializeField] GameObject uiParent;

    bool isMenuOpen;


    public virtual void Start()
    {
        uiParent.SetActive(false);
    }

    private void Update()
    {
        if (!isMenuOpen) return;

        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1))
            
                CloseMenu();
            
    }

    public virtual void OpenMenu() 
    {
        if (isMenuOpen) return;

        isMenuOpen = true;  

        uiParent.SetActive(true);
        GM.Instance.GameManager.isUIOpened = true;
    }

    public virtual void CloseMenu()
    {
        if (!isMenuOpen) return;

        isMenuOpen = false; 

        uiParent.SetActive(false);
        GM.Instance.GameManager.isUIOpened = false;
    }



}
