using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarMantis : MonoBehaviour
{
    private EntityStats EntityStats;
    public Image Fill;
     
    void Start()
    {

    }

     
    void Update()
    {
        Fill.fillAmount = (float)GM.Instance.GameManager.MantisHealth / GM.Instance.GameManager.MantisMaxHealth;

         

    }

}
