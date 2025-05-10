using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Item Effect", menuName = "Data/Item Effect/Heal")]

public class Effect_Heal : ItemEffect
{
    [SerializeField] int HealAmount;
    [SerializeField] int FirePowerUP;
    [SerializeField] int ArmorUP;
    [SerializeField] int CritChanceUP;
    [SerializeField] int CritDamageUP;

    public override void ExecuteEffect(Transform _position)
    {
        PlayerStats player = PlayerManager.Instance.player.GetComponent<PlayerStats>();
        player.CurrentHP += HealAmount;

        if (player.CurrentHP > player.GetMaxHealth()) 
        player.CurrentHP = player.GetMaxHealth();   

        player.FirePower.AddModifier(FirePowerUP); 
        player.Armor.AddModifier(ArmorUP);
        player.CriticalChance.AddModifier(CritChanceUP);
        player.CriticalDamage.AddModifier(CritDamageUP);    
         


    }
}
