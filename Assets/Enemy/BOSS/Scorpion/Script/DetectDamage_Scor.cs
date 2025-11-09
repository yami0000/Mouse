using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectDamage_Scor : MonoBehaviour
{
    [Header("Hitboxes")]
    [SerializeField] private Collider2D swingHitbox1;
    [SerializeField] private Collider2D swingHitbox2;
    [SerializeField] private Collider2D meleeHitbox;

    [SerializeField] private EnemyStats EnemyStats;
    [SerializeField] private Enemy enemy;

    private bool hasDealtDamage = false;
    private void Start()
    {
        // disable all hitboxes at start
        swingHitbox1.enabled = false;
        swingHitbox2.enabled = false;
        meleeHitbox.enabled = false;
        EnemyStats = enemy.GetComponent<EnemyStats>();

    }

    public void EnableSwingHitbox1()
    {
        swingHitbox1.enabled = true;
        hasDealtDamage = false;
    }
    public void DisableSwingHitbox1() => swingHitbox1.enabled = false;

    public void EnableSwingHitbox2()
    {
        swingHitbox2.enabled = true;
        hasDealtDamage = false;
    }
    public void DisableSwingHitbox2() => swingHitbox2.enabled = false;

    public void EnableMeleeHitbox()
    {
        meleeHitbox.enabled = true;
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
