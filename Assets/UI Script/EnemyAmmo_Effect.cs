using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity.Attributes;

public class EnemyAmmo_Effect : MonoBehaviour
{
    private Enemy Enemy;
    private EnemyStats EnemyStats;
    [SerializeField] private bool Bounce;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float minSpeed ;
    [SerializeField] private float maxSpeed ;
    [SerializeField] private float minAngle ;
    [SerializeField] private float maxAngle;


    public void Initialize(Enemy enemy)
    {

        EnemyStats = enemy.GetComponent<EnemyStats>();

    }
    public void Initialize2(Enemy enemy)
    {

        Enemy = enemy;

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {



        if (collision.GetComponent<Player>() != null)
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();


            EnemyStats.DoDamage(player);


            Destroy(gameObject);

        }

        if (collision.CompareTag("Shield"))
            Destroy(gameObject);

            if (Bounce)
        {
            if (collision.CompareTag("Ground"))
            {
                Vector2 groundPos = gameObject.transform.position;

                Destroy(gameObject);

                // Spawn 2 projectiles
                SpawnProjectile(groundPos, true);   // left
                SpawnProjectile(groundPos, false);  // right

                 
                
            }
        }

    }

    private void SpawnProjectile(Vector2 spawnPos, bool isLeft)
    {
        // Instantiate projectile
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        float angle = Random.Range(minAngle, maxAngle);
        float speed = Random.Range(minSpeed, maxSpeed);  // renamed variables

        // Convert angle to radians
        float rad = angle * Mathf.Deg2Rad;

        // Direction vector
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        if (isLeft) dir.x *= -1;  // flip to left if needed

        // Add velocity to Rigidbody2D
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        { 
            rb.velocity = dir * speed; //  directly set initial velocity
        }


        EnemyAmmo_Effect _Ammo = proj.GetComponent<EnemyAmmo_Effect>();
        if (_Ammo != null)
            _Ammo.Initialize(Enemy);

        Destroy(proj,5);

        Destroy(gameObject, 5);
    }

}
