using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;



public class Effect_Heal : ItemEffect
{
    [SerializeField] int HealAmount;
    [FormerlySerializedAs("FirePowerUP")]
    [SerializeField] int DamageUP;
    [SerializeField] int ArmorUP;
    [SerializeField] int CritChanceUP;
    [SerializeField] int CritDamageUP;
     
    public override void ExecuteEffect(Transform _position)
    {
        PlayerStats player = PlayerManager.Instance.player.GetComponent<PlayerStats>();
        player.HealEntity(HealAmount);

        player.Damage.AddModifier(DamageUP); 
        player.Armor.AddModifier(ArmorUP);
        player.CriticalChance.AddModifier(CritChanceUP);
        player.CriticalDamage.AddModifier(CritDamageUP);    
         


    }

  
}
