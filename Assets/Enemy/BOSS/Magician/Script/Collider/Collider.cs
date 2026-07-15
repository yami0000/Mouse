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
    [SerializeField]private float A;
    [SerializeField]private float B;

    private void Start()
    {
        cd = GetComponent<Collider2D>();
        cd.enabled = false;
        StartCoroutine(ColliderAdjustment());

    }
    IEnumerator ColliderAdjustment() 
    {
      yield return  new WaitForSeconds(A);
        cd.enabled = true;
      yield return new WaitForSeconds(B-A);
        cd.enabled = false;
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

                AttackedOnce = true;
            }


        }

    }
}
