using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSwamp : MonoBehaviour
{
    private EntityStats stats => GetComponentInParent<EntityStats>();

    private HashSet<Entity> entitysInSwamp = new HashSet<Entity>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        EntityStats entityStats = collision.GetComponent<EntityStats>();

        if (entity != null && entityStats != null && !entitysInSwamp.Contains(entity))
        {
            entitysInSwamp.Add(entity);
            stats.DoDamage(entityStats); // Initial damage
            StartCoroutine(DamageOverTime(entity, entityStats));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null && entitysInSwamp.Contains(entity))
        {
            entitysInSwamp.Remove(entity);
        }
    }

    private IEnumerator DamageOverTime(Entity entity, EntityStats entityStats)
    {
        while (entitysInSwamp.Contains(entity))
        {
            yield return new WaitForSeconds(1.5f);

            if (entity != null && entityStats != null && entitysInSwamp.Contains(entity))
            {
                stats.DoDamage(entityStats);
            }
        }
    }
}
