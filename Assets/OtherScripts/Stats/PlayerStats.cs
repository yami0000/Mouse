using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats  : EntityStats
{   
    private Player Player;
    protected override void Start()
    {
        base.Start();

        Player = GetComponent<Player>();
    }

    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);

        Player.damageEffect();
    }

    protected override void Die()
    {
        base.Die();

        Player.Die();
    }
}
