using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide_ChargeAmmo : AmmoEffect
{
    [Header("Damage Settings")]
    [SerializeField] private AnimationCurve damageCurve;

    private PlayerStats PlayerStats;
    protected Transform enemy;
    private float ChargeTime = 0;

    private void Start()
    {
        if(PlayerManager.Instance.player.currentChargeTimer>0)
        {
            ChargeTime = PlayerManager.Instance.player.currentChargeTimer;
           
            PlayerManager.Instance.player.currentChargeTimer = 0;

        }
        PlayerStats = PlayerManager.Instance.player.GetComponent<PlayerStats>();
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {

            EnemyStats enemyTarget = collision.GetComponent<EnemyStats>();
            enemy = collision.GetComponent<Enemy>().transform;



            float calculatedMultiplier = damageCurve.Evaluate(ChargeTime);

            PlayerStats.DoDamage(enemyTarget, calculatedMultiplier);
            ChargeTime = 0;

            finalDirection = ((Vector2)transform.position - lastFramePosition).normalized;
            int Dmg = PlayerStats.DoDamage(enemyTarget, calculatedMultiplier);
            _OnDestroy(finalDirection, Dmg);

            Destroy(gameObject);
        }
    }
}
