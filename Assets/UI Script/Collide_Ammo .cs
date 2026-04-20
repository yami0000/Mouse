using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide_Ammo : AmmoEffect
{
    private PlayerStats PlayerStats;
    protected Transform enemy;
   
     
    

    private void Start()
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
            finalDirection = ((Vector2)transform.position - lastFramePosition).normalized;
            int Dmg = PlayerStats.DoDamage(enemyTarget);
            _OnDestroy(finalDirection, Dmg);

            Destroy(gameObject);
        }
    }

   

   
}
