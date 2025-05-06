using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Item Effect", menuName = "Data/Item Effect/Heal")]

public class Effect_Heal : ItemEffect
{
    [SerializeField] int HealAmount;

    public override void ExecuteEffect(Transform _position)
    {
        PlayerStats player = PlayerManager.Instance.player.GetComponent<PlayerStats>();
        player.CurrentHP += HealAmount;


    }
}
