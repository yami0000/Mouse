using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide_SelfAimAmmoEnemy : MonoBehaviour
{
    private Enemy enemy;
    private EnemyStats EnemyStats;

    private Rigidbody2D rb;
    private Transform player;

    [Header("SelfAim Attribute")]
    public float slowDownRate = 2f;     // how quickly it loses velocity
    public float homingDelay = 0.5f;    // delay before homing starts
    public float homingAcceleration = 10f; // how fast it speeds up toward target
    public float maxSpeed = 15f;        // max speed when homing
    private float timer;



    private void Start()
    {
        player = PlayerManager.Instance.player.transform;
        rb = GetComponent<Rigidbody2D>();
    }
    public void Initialize(Enemy enemy)
    {

        EnemyStats = enemy.GetComponent<EnemyStats>();

    }

    void Update()
    {
        timer += Time.deltaTime;

        
            if (timer < homingDelay)
            {
                // phase 1 ¡ú slow down
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, slowDownRate * Time.deltaTime);
            }
            else
            {
                // phase 2 ¡ú accelerate toward player
                Vector2 dir = (player.position - transform.position).normalized;
                rb.velocity = Vector2.MoveTowards(rb.velocity, dir * maxSpeed, homingAcceleration * Time.deltaTime);
            }
         
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {



        if (collision.GetComponent<Player>() != null)
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();


            EnemyStats.DoDamage(player);


            Destroy(gameObject);
        }
        if (collision.CompareTag("Ground"))
            Destroy(gameObject);

    }
}
