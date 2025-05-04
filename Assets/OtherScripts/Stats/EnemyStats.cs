using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : EntityStats

{
    private Enemy enemy;
    private ItemDrop myDropSystem;

    [Header("level details")]
    [SerializeField] private int level = 1;

    [Range(0f,1f)]
    [SerializeField] private float percantageModifier;
    protected override void Start()
    {
        ApplyLevelModifier();
        base.Start();

        enemy = GetComponent<Enemy>();
        myDropSystem = GetComponent<ItemDrop>();

    }

    private void ApplyLevelModifier()
    {
        Modify(MaxHP);
        Modify(Damage);

        Modify(FireDamage);
        Modify(FrostDamage);
        Modify(PoisonDamage);   
        Modify(LightningDamage);

        Modify(Armor);
        Modify(Intelligence);
    }

    private void Modify(Stats _stats) 
    {
        for (int i=1 ; i < level; i++)
            {
            float modifier = _stats.GetValue() * percantageModifier;

            _stats.AddModifier(Mathf.RoundToInt(modifier));
            }
    
    
    }
    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);

        enemy.damageEffect();
    }

    protected override void Die()
    {
        base.Die();

        enemy.Die();

        myDropSystem.GenerateDrop();
    }


}
