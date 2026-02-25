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

        if(_damage <= 0)
            return;

        Player.damageEffect();
    }

    protected override void Die()
    {
        base.Die();

        Player.Die();
    }
}
