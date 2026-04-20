using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn;

public class Collide_SelfAimAmmo : AmmoEffect
{
    private Rigidbody2D rb;

    [Header("SelfAim Attribute")]
    public float slowDownRate = 2f;     // how quickly it loses velocity
    public float homingDelay = 0.5f;    // delay before homing starts
    public float homingAcceleration = 10f; // how fast it speeds up toward target
    public float maxSpeed = 15f;        // max speed when homing
    private float timer;

    [Header("Damage Calculation")]
    private Transform player;
    private PlayerStats PlayerStats;
    protected Transform enemy;

    [Header("Enemy Detection")]
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private float enemyCheckDistance;
    public Enemy _enemy;
    void Start()
    {
        player = PlayerManager.Instance.player.transform;
        rb = GetComponent<Rigidbody2D>();
        PlayerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();
        pinpointClosestEnemy();
    }


    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (_enemy != null)
        {
            if (timer < homingDelay)
            {
                // phase 1 ¡ú slow down
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, slowDownRate * Time.deltaTime);
            }
            else
            {
                // phase 2 ¡ú accelerate toward enemy
                Vector2 dir = (_enemy.transform.position - transform.position).normalized;
                rb.velocity = Vector2.MoveTowards(rb.velocity, dir * maxSpeed, homingAcceleration * Time.deltaTime);
            }
        }
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
        if (collision.CompareTag("Ground"))
            Destroy(gameObject);
    }

    public void pinpointClosestEnemy()
    {

        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, enemyCheckDistance, whatIsEnemy);

        if (hits.Length == 0)
        {
            _enemy = null;
            return;
        }


        float minDistance = Mathf.Infinity;
        Vector2 pos = transform.position;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject)
                continue;




            Enemy enemy = hit.gameObject.GetComponent<Enemy>();


            float distance = Vector2.Distance(pos, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                _enemy = enemy;
            }

        }
    }
}
