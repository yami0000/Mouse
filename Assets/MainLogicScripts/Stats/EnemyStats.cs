using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : EntityStats

{
    private Enemy enemy;
    private ItemDrop myDropSystem;

    //[Header("level details")]
    

    //[Range(0f,1f)]
    //[SerializeField] private float percantageModifier;
    protected override void Start()
    {
         
        base.Start();

        enemy = GetComponent<Enemy>();
        myDropSystem = GetComponent<ItemDrop>();

    }

 

 
    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);

        if (_damage <= 0)
            return;

        enemy.damageEffect();
    }

    protected override void Die()
    {
        base.Die();



        enemy.Die();

        myDropSystem.GenerateDrop();
    }


}
