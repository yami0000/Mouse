using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide_RevolverAmmo : MonoBehaviour
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



            DoDamage(enemyTarget);

            Destroy(gameObject);
        }
        if (collision.CompareTag("Ground"))
            Destroy(gameObject);

    }
    private  void DoDamage(EntityStats _targetStats)
    {
        // if (Avoid(_targetStats))
        // return;

        int totalDamage = Mathf.RoundToInt((PlayerStats.Damage.GetValue() + PlayerStats.FirePower.GetValue())*Random.Range(0.8f, 1.40f));

        if (PlayerStats.CanCritical())
            totalDamage = PlayerStats.CalculateCrit(totalDamage);

        totalDamage = PlayerStats.ArmorSystem(_targetStats, totalDamage);

        _targetStats.TakeDamage(totalDamage);
        PlayerStats.DoElementDamage(_targetStats);

        Debug.Log(totalDamage);
    }//特性：造成80%-140%的伤害。此处因为特殊规则将伤害计算函数单独抽出使用。
}
