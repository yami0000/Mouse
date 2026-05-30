using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Weapon : MonoBehaviour
{
    public GameObject Weapon;
    public Image UI_weapon;


    private void Start()
    {
        UI_weapon.sprite = null;
        UI_weapon.color = Color.clear;
    }
    private void Update()
    {
        UpdateGraphic();
    }
    private void UpdateGraphic()
    {
       SpriteRenderer UI_Weapon = Weapon.GetComponent<SpriteRenderer>();
        if(UI_weapon.sprite == UI_Weapon.sprite) return;

        if (PlayerWeaponHolder.Instance.isHolding)
        {
            
            
            UI_weapon.sprite = UI_Weapon.sprite;
            UI_weapon.color = Color.white;
            UI_weapon.preserveAspect = true;
        }
        
        if(!PlayerWeaponHolder.Instance.isHolding)
        {

            UI_weapon.sprite = null;    
            UI_weapon.color = Color.clear;
        }

        
    }
}
