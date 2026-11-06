using UnityEngine;

/// <summary>
/// Put this on the GameObject that holds the NPC's Animator (usually a child).
/// Add an Animation Event calling "AnimationTrigger" at the end of a special clip
/// so SpecialAction's "AnimationEvent" finish mode knows the clip is done.
/// Works for any NPC (unlike ChefAnimation, which is chef-only).
/// </summary>
public class NPCAnimationEvents : MonoBehaviour
{
    private NPC npc;

    private void Awake()
    {
        npc = GetComponentInParent<NPC>();
    }

    private void AnimationTrigger()
    {
        if (npc != null) npc.AnimationTrigger();
    }
}
