using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo_Effect : Entity
{
    private PlayerStats PlayerStats;
    protected Transform enemy;

     new private void Start()
    {
        PlayerStats =  PlayerManager.Instance.player.GetComponent<PlayerStats>();
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Enemy>()!=null)
        {
            
            EnemyStats enemyTarget = collision.GetComponent<EnemyStats>();
            enemy = collision.GetComponent<Enemy>().transform;

            
             
            PlayerStats.DoDamage(enemyTarget);

            Destroy(gameObject);
        }
    }
}
