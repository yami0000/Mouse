using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCompanion : MonoBehaviour
{
    public static PlayerCompanion Instance;
    [SerializeField] private GameObject DroneA;
    private GameObject Companion;


    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        else
            Instance = this;
    }
    public void EquipCompanion(ItemData_Equipment equipmentData)
    { 
    if(equipmentData.itemName == "DroneA")
    
          Companion = Instantiate(DroneA,PlayerManager.Instance.player.transform.position,Quaternion.identity);
             

    }

    public void UnEquipCompanion()
    {
        Destroy(Companion);
        Companion = null;


    }

}
