using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ButtonInUI : MonoBehaviour
{
    [SerializeField] private GameObject Inv;
    [SerializeField] private GameObject Sta;

    private void Start()
    {
        inv();
    }

    public void inv() 
    {
        Sta.SetActive(false);
        Inv.SetActive(true);
    }

    public void sta() 
    {
        Inv.SetActive(false);
        Sta.SetActive(true);
    }
}
