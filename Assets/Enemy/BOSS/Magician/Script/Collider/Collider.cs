using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider : MonoBehaviour
{
    private bool AttackedOnce = false;
    private EnemyStats EnemyStats;
    [Header("Enable/Disable Collider2D")]
    [Tooltip("A:start enable cd  B:start disable cd")]
    private Collider2D cd;
    [SerializeField] private float A;
    [SerializeField] private float B;

    [Header("Destroy")]
    [SerializeField]private bool CanBeDestroyed;

    private void Start()
    {
        cd = GetComponent<Collider2D>();
        cd.enabled = false;
        StartCoroutine(ColliderAdjustment());

    }
    IEnumerator ColliderAdjustment()
    {
        yield return new WaitForSeconds(A);
        cd.enabled = true;
        yield return new WaitForSeconds(B - A);
        cd.enabled = false;
    }

    // Force the collider on/off, cancelling the A/B timer.
    // Used when the barrage launches a projectile long after B has passed.
    public void SetColliderEnabled(bool on)
    {
        StopAllCoroutines();
        if (cd == null) cd = GetComponent<Collider2D>();
        cd.enabled = on;
    }

    public void Initialize(Enemy enemy)
    {
        EnemyStats = enemy.GetComponent<EnemyStats>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {



        if (collision.GetComponent<Player>() != null)
        {
            if (!AttackedOnce)
            {
                PlayerStats player = collision.GetComponent<PlayerStats>();

                EnemyStats.DoDamage(player);

                if (CanBeDestroyed)
                    Destroy(gameObject);

                AttackedOnce = true;
            }


        }

    }
}
