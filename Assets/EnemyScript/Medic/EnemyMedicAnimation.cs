using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMedicAnimation : MonoBehaviour
{
    private Enemy_Medic enemy => GetComponentInParent<Enemy_Medic>();

    private void AnimationTrigger_Medic()
    {
        enemy.AnimationTrigger_Medic();
    }

    private void AttackPreparetion_Medic()
    {
        enemy.AttackPreparetion_Medic();
    }
}
