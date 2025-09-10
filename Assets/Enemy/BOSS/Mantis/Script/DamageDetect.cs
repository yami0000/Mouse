using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDetect : MonoBehaviour
{
    [Header("Hitboxes")]
    [SerializeField] private Collider2D dashHitbox;
    [SerializeField] private Collider2D slamHitbox;

    [SerializeField] private EnemyStats EnemyStats;
    [SerializeField]private Enemy enemy;

    private bool hasDealtDamage = false;
    private void Start()
    {
        // disable all hitboxes at start
        dashHitbox.enabled = false;
        slamHitbox.enabled = false;
        EnemyStats = enemy.GetComponent<EnemyStats>();

    }

    public void EnableDashHitbox()
    {
        dashHitbox.enabled = true; 
        hasDealtDamage=false;
    }
    public void DisableDashHitbox() => dashHitbox.enabled = false;

    public void EnableSlamHitbox()
    {
        slamHitbox.enabled = true;
        hasDealtDamage = false;
    }
    
    public void DisableSlamHitbox() => slamHitbox.enabled = false;


    private void OnTriggerEnter2D(Collider2D collision) 
    {
         
        if (!hasDealtDamage && collision.GetComponent<Player>() != null)

        {
            
            PlayerStats player = collision.GetComponent<PlayerStats>();


            hasDealtDamage = true;
            EnemyStats.DoDamage(player);

             
        }


        }
}
