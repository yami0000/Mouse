using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide_RevolverAmmo : AmmoEffect
{
    private PlayerStats PlayerStats;
    protected Transform enemy;

    private void Start()
    {
        PlayerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {

            EnemyStats enemyTarget = collision.GetComponent<EnemyStats>();
            enemy = collision.GetComponent<Enemy>().transform;

            float damageMultiplier = Random.Range(0.8f, 1.40f);

            PlayerStats.DoDamage(enemyTarget,damageMultiplier);


            finalDirection = ((Vector2)transform.position - lastFramePosition).normalized;
            int Dmg = PlayerStats.DoDamage(enemyTarget, damageMultiplier);
            _OnDestroy(finalDirection,Dmg);

            Destroy(gameObject);
        }
        if (collision.CompareTag("Ground"))
            Destroy(gameObject);

    }
 
}
