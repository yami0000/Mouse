using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode_Grenade : MonoBehaviour
{
    private EntityStats Stats;
     
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();
    public void Initialize(EntityStats stats)
    {
        Stats = stats;
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitEnemies.Contains(collision.gameObject)) return;

        if (collision.TryGetComponent<Enemy>(out Enemy enemyComponent))
        {
            EnemyStats enemyTarget = collision.GetComponent<EnemyStats>();


            hitEnemies.Add(collision.gameObject);

            Stats.DoDamage(enemyTarget);
        }
    }
}
