using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.Rendering.DebugUI;

public class PlayerStats  : EntityStats
{   
    private Player Player;
    public event Action OnHealthChanged;
    protected override void Start()
    {
        base.Start();

        Player = GetComponent<Player>();
    }

    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);
        OnHealthChanged?.Invoke();

        if (_damage <= 0)
            return;

        Player.damageEffect();
    }
    public override void HealEntity(int HealAmount)
    {
        base.HealEntity(HealAmount);
        OnHealthChanged?.Invoke();
        CurrentHP += HealAmount;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP.GetValue());
    }

 

    protected override void Die()
    {
        base.Die();

        Player.Die();
    }

}
