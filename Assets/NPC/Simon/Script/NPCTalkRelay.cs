using UnityEngine;

/// <summary>
/// Edge trigger for an NPCController interrupt. Put this on the NPC's interact
/// collider (a trigger Collider2D on the NPC itself, or a child of it, so it
/// moves with the NPC). When the player is inside and presses E, it raises the
/// named interrupt on the controller; the controller does the rest (freeze the
/// routine, run the interrupt body, resume).
///
/// This keeps collision + input detection in DETECTION (where it belongs) and
/// behaviour in NPCController, mirroring the LanguageButton relay pattern. Use
/// this instead of NPCDialogue on controller-driven NPCs (don't put both on the
/// same object, or E would fire twice).
/// </summary>
public class NPCTalkRelay : DETECTION
{
    [Tooltip("The controller to interrupt. If left empty, found on this object or a parent.")]
    [SerializeField] private NPCController controller;

    [Tooltip("Must match the NPCInterrupt.id you want to fire.")]
    [SerializeField] private string interruptId = "talk";

    private void Awake()
    {
        if (controller == null)
            controller = GetComponentInParent<NPCController>();
    }

    public override void Interact()
    {
        base.Interact();
        if (controller != null)
            controller.Raise(interruptId);
    }
}
