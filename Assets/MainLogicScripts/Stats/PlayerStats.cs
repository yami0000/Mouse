using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.Rendering.DebugUI;

public class PlayerStats  : EntityStats
{   
    private Player Player;
    public event Action OnHealthChanged;

    [Header("Temporary Health")]
    [Tooltip("How much of Max Health, in percent, temporary health loses each second. It loses none while still being delivered.")]
    [SerializeField, Min(0)] private float temporaryHealthDecay = 4f;

    private float temporaryHealthIncoming;
    private float temporaryHealthRate;

    /// <summary>Takes damage before CurrentHP. Only ever comes from GiveTemporaryHealth, and stays between 0 and Max Health.</summary>
    public float TemporaryHealth { get; private set; }
    public float TemporaryHealthFraction => TemporaryHealth / Mathf.Max(1, GetMaxHealth());

    /// <summary>Any damage above 0, whether or not temporary health took it.</summary>
    public event Action OnDamageTaken;
    /// <summary>Temporary health reached 0 with none left to deliver.</summary>
    public event Action OnTemporaryHealthGone;

    protected override void Start()
    {
        base.Start();

        Player = GetComponent<Player>();
    }

    protected override void Update()
    {
        base.Update();
        UpdateTemporaryHealth();
    }

    /// <summary>Adds _amount over _seconds, on top of anything still being delivered.</summary>
    public void GiveTemporaryHealth(float _amount, float _seconds)
    {
        if (_amount <= 0f) return;

        if (_seconds <= 0f)
        {
            TemporaryHealth = Mathf.Min(TemporaryHealth + _amount, GetMaxHealth());
            return;
        }

        temporaryHealthIncoming += _amount;
        temporaryHealthRate = temporaryHealthIncoming / _seconds;
    }

    private void UpdateTemporaryHealth()
    {
        float maxHealth = GetMaxHealth();

        if (temporaryHealthIncoming > 0f)
        {
            float delivered = Mathf.Min(temporaryHealthIncoming, temporaryHealthRate * Time.deltaTime);
            temporaryHealthIncoming -= delivered;
            TemporaryHealth = Mathf.Min(TemporaryHealth + delivered, maxHealth);
        }
        else if (TemporaryHealth > 0f)
        {
            float lost = temporaryHealthDecay * 0.01f * maxHealth * Time.deltaTime;
            TemporaryHealth = Mathf.Max(0f, Mathf.Min(TemporaryHealth, maxHealth) - lost);

            if (TemporaryHealth <= 0f)
                OnTemporaryHealthGone?.Invoke();
        }
    }

    protected override int AbsorbDamage(int _damage)
    {
        if (_damage <= 0) return _damage;

        OnDamageTaken?.Invoke();

        if (TemporaryHealth <= 0f) return _damage;

        if (_damage < TemporaryHealth)
        {
            TemporaryHealth -= _damage;
            return 0;
        }

        float overflow = _damage - TemporaryHealth;
        TemporaryHealth = 0f;

        if (temporaryHealthIncoming <= 0f)
            OnTemporaryHealthGone?.Invoke();

        if (Skill.Instance != null && Skill.Instance.ShieldOfTheInferno)
            overflow *= 1f - SkillList.FindMechanism(SkillMechanism.ShieldOfTheInferno).infernoOverflowReduction * 0.01f;

        return Mathf.CeilToInt(overflow);
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
        CurrentHP += HealAmount;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP.GetValue());
        // After the change, so the health bar shows the healed value.
        OnHealthChanged?.Invoke();
    }

 

    protected override void Die()
    {
        base.Die();

        Player.Die();
    }

    public override void Revive()
    {
        base.Revive();

        bool hadTemporaryHealth = TemporaryHealth > 0f || temporaryHealthIncoming > 0f;
        TemporaryHealth = 0f;
        temporaryHealthIncoming = 0f;

        if (hadTemporaryHealth)
            OnTemporaryHealthGone?.Invoke();

        OnHealthChanged?.Invoke();
    }
}
