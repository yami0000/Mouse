using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PassiveSkillHandler : MonoBehaviour
{
    private readonly List<PassiveEffectInstance> _instances = new();
    private PlayerStats _stats; // your existing stats component

    void Awake() => _stats = GetComponent<PlayerStats>();

    void OnEnable()
    {
        // Subscribe to any stat change that passives care about
        _stats.OnHealthChanged += HandleStatChange;
        
    }

    void OnDisable()
    {
        _stats.OnHealthChanged -= HandleStatChange;
        
    }

    // Called by EquipmentManager when item is equipped
    public void RegisterItem(ItemData item)
    {
        if (item.passiveEffects == null || item.passiveEffects.Length == 0) return;

        

        foreach (var so in item.passiveEffects)
        {
            if (so == null)
            {
                
                continue;
            }

            Debug.Log($"[Passive] Registering: {so.name}");

            var instance = new PassiveEffectInstance(so, gameObject);
            _instances.Add(instance);
            instance.Evaluate(BuildContext()); // evaluate immediately on equip
            Debug.Log($"[Passive] Instance active after first evaluate: {instance.IsActive}");

        }
    }

    // Called by EquipmentManager when item is unequipped
    public void UnregisterItem(ItemData item)
    {
        _instances.RemoveAll(inst =>
        {
            if (item.passiveEffects.Contains(inst.Definition))
            {
                inst.ForceRemove();
                return true;
            }
            return false;
        });
    }

    private void HandleStatChange()
    {
        var ctx = BuildContext();
        foreach (var inst in _instances)
            inst.Evaluate(ctx);
    }

    private PassiveContext BuildContext() => new PassiveContext
    {
        currentHP = _stats.CurrentHP,
        maxHP = _stats.MaxHP.GetValue(),
    };
}
