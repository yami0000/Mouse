using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAmmo_Effect : MonoBehaviour
{
   // private Enemy_Medic enemy;
    private EnemyStats EnemyStats;
    private Enemy_Medic mySelf;
    public void initialize(Enemy_Medic enemy) 
    {
    EnemyStats = enemy.GetComponent<EnemyStats>();
        mySelf = enemy;
    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       if(collision.GetComponent<Enemy>() != null)
        {
             if(collision.gameObject == mySelf.gameObject)
                return;

          

         EnemyStats _enemy = collision.GetComponent<EnemyStats>();

            EnemyStats.Heal(_enemy);

            mySelf.ParticleSystem.ExcuteHealingFX();

            Destroy(gameObject);
        
        
        }
    }


}
