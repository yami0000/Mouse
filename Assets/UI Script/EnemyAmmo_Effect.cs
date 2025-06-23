using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAmmo_Effect : MonoBehaviour
{
     private Enemy_Bee enemy;
    private EnemyStats EnemyStats;



    public void Initialize(Enemy enemy) 
    { 
    
     EnemyStats = enemy.GetComponent<EnemyStats>();
    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
       

         
        if (collision.GetComponent<Player>() != null)
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();

             
            EnemyStats.DoDamage(player);


            Destroy(gameObject);
        }


    }

 
}
