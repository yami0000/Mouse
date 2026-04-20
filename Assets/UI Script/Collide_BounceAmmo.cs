using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide_BounceAmmo : AmmoEffect
{
    private PlayerStats PlayerStats;
    protected Transform enemy;
    private Rigidbody2D rb;


    private Vector2 lastVelocity;
    private float initialSpeed;
    

     private void Start()
    {
         rb = GetComponent<Rigidbody2D>();
        lastVelocity = rb.velocity;
        initialSpeed = rb.velocity.magnitude;
        
        PlayerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {

            EnemyStats enemyTarget = collision.GetComponent<EnemyStats>();
            enemy = collision.GetComponent<Enemy>().transform;



            PlayerStats.DoDamage(enemyTarget);
            finalDirection = ((Vector2)transform.position - lastFramePosition).normalized;
            int Dmg = PlayerStats.DoDamage(enemyTarget);
            _OnDestroy(finalDirection, Dmg);
            Destroy(gameObject);
        }
        //if (collision.CompareTag("Ground"))
           // Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
           
            //speed = lastVelocity.magnitude;

            
            Vector2 wallNormal = collision.contacts[0].normal;

           
            Vector2 direction = Vector2.Reflect(lastVelocity.normalized, wallNormal);

            Debug.Log(direction);

            rb.velocity = direction * initialSpeed;
            lastVelocity = rb.velocity;
        }
    }
}
