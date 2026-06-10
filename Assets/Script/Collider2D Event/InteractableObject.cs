using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject :DETECTION
{
    [SerializeField] private string DialogueName;

    [Tooltip("Dialogue starts after t seconds")]
    [SerializeField] private float t;
    [SerializeField] private bool CanBeInteractedRepeately;

    private bool _hasTriggered = false;
    private Collider2D cd;

    public override void Interact()
    {
        if (_hasTriggered) return;
        _hasTriggered = true;

        base.Interact();
        NarrativeManager.Instance.RequestDialogue(DialogueName, t);

        if (!CanBeInteractedRepeately)
            cd.enabled = false;

    }

    private void Start()
    {
        cd = GetComponent<Collider2D>();

    }

    
}
