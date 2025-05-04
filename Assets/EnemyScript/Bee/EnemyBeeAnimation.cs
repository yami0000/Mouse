using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBeeAnimation : MonoBehaviour
{
    private Enemy_Bee enemy => GetComponentInParent<Enemy_Bee>();

    private void AnimationTrigger_Bee()
    {
        enemy.AnimationTrigger_Bee();
    }

    private void AttackPreparetion_Bee()
    {
        enemy.AttackPreparetion_Bee();
    }
}
