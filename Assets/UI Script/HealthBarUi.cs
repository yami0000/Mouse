using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class HealthBarUi : MonoBehaviour
{
    private EntityStats EntityStats;
    public Image Fill;
    [SerializeField] private Player player;  
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Fill.fillAmount = (float)player.stats.CurrentHP / player.stats.GetMaxHealth();
         
        

    }

 

    

   
}
