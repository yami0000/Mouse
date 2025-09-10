using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArmorHolder : MonoBehaviour
{
    public static PlayerArmorHolder Instance;

    public GameObject Armor;



    public bool isWearing = false;





    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        else
            Instance = this;
    }


    public void EquipArmor(ItemData_Equipment armorData)
    {



        SpriteRenderer armor = Armor.GetComponent<SpriteRenderer>();



        if (armor != null)
        {
            armor.sprite = armorData.icon;

        }




        isWearing = true;

    }

    public void UnEquipArmor()
    {
        SpriteRenderer armor = Armor.GetComponent<SpriteRenderer>();

        armor.sprite = null;

        isWearing = false;


    }
}
