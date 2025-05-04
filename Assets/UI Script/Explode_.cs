using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explode_ : MonoBehaviour
{
    [SerializeField] private Enemy_SelfDes enemy;
    private EnemyStats EnemyStats;
    [SerializeField] private Explode_ explode;
    [SerializeField] private float effectiveTime;

    public CapsuleCollider2D cd { get; private set; }


    private void Start()
    {
        cd = GetComponent<CapsuleCollider2D>();
        EnemyStats = enemy.GetComponent<EnemyStats>();

        StartCoroutine(RestrictionOfExplosion());
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();


            EnemyStats.DoDamage(player);


            explode.cd.enabled = false;
        }
    }

    IEnumerator RestrictionOfExplosion()
    {
          float t = 0;

        while(t < effectiveTime)
        {
            t += Time.deltaTime;
            yield return null;

        }

        explode.cd.enabled = false;








    }
}
