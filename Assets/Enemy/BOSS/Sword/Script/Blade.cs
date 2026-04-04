using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blade : MonoBehaviour
{
    private Enemy enemy;
    private EnemyStats EnemyStats;
    public float speed;
    public Vector3 direction;
    public float slowdownTime;
    public LayerMask groundLayer;  
    public bool AttackedOnce = false;

    private bool isSlowingDown = false;

    private void Update()
    {
        if (!isSlowingDown)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    public void Initialize(Enemy enemy) 
    {
        EnemyStats = enemy.GetComponent<EnemyStats>();
    }
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (!isSlowingDown)
            if (collision.CompareTag("Ground"))
            { 
                StartCoroutine(StopBlade(collision.transform));
            }

        if(!AttackedOnce)
        if (collision.GetComponent<Player>() != null)
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();

            EnemyStats.DoDamage(player);

            AttackedOnce = true;

        }
    }

    IEnumerator StopBlade(Transform ground)
    {
        isSlowingDown = true;
        float startSpeed = speed;
        float elapsed = 0;

        while (elapsed < slowdownTime)
        {
          
            if (this == null) yield break;

            elapsed += Time.deltaTime;

          
            speed = Mathf.Lerp(startSpeed, 0, elapsed / slowdownTime);

            
            transform.position += direction * speed * Time.deltaTime;
            yield return null;
        }

        speed = 0;

        
    }
}

