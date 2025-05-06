using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponHolder : MonoBehaviour
{

    

    public static PlayerWeaponHolder Instance;

    public GameObject Weapon;
 
     

    public bool isHolding = false;

  

 

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        else
            Instance = this;
    }
 

    public void EquipWeapon(ItemData_Equipment weaponData)
    {

        

        SpriteRenderer weapon = Weapon.GetComponent<SpriteRenderer>(); 

         

        if (weapon != null  )
        {
            weapon.sprite = weaponData.icon;

        }

       

 
        isHolding = true;   
        
    }

    public void UnEquipWeapon()
    {
        SpriteRenderer weapon = Weapon.GetComponent<SpriteRenderer>();

        weapon.sprite = null;

        isHolding = false;   


    }


 



}