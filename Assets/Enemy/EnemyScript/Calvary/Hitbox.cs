using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private Collider2D meleeHitbox;

    [SerializeField] private EnemyStats EnemyStats;
    [SerializeField] private Enemy enemy;
    private bool hasDealtDamage = false;

    private void Start()
    {

        meleeHitbox.enabled = false;

        EnemyStats = enemy.GetComponent<EnemyStats>();

    }

    public void EnableMeleeHitbox()
    {
        meleeHitbox.enabled = true;
        Debug.Log("Appeared!");
        hasDealtDamage = false;
    }
    public void DisableMeleeHitbox() => meleeHitbox.enabled = false;

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
