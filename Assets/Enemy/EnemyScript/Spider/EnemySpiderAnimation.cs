using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpiderAnimation : MonoBehaviour
{
    private Enemy_Spider enemy => GetComponentInParent<Enemy_Spider>();

    private void AnimationTrigger() 
    {
    enemy.AnimationTrigger();
    }
    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemy.attackCheck.position, enemy.attackCheckRadius);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Player>() != null)
            {
                PlayerStats target = hit.GetComponent<PlayerStats>();
                enemy.stats.DoDamage(target);
            }
        }

    }
    private void AttackPreparetion() 
    {
        enemy.AttackPreparetion();
    }


}
