using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefAnimation : MonoBehaviour
{
    private NPCchef npc => GetComponentInParent<NPCchef>();

    private void AnimationTrigger()
    {
        npc.AnimationTrigger();
    }
}
