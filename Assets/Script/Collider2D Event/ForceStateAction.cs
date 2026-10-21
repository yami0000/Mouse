using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Forces enemies / NPCs into one chosen state, e.g. puts a boss into its battle state or makes an
/// NPC idle. Their own logic carries on from there, so they may move on to other states by themselves.
/// The state list in the inspector is read from the targets (see ForceStateActionDrawer).
/// </summary>
[System.Serializable]
public class ForceStateAction
{
    [Tooltip("Enemies / NPCs to put into the state.")]
    public List<Entity> targets = new List<Entity>();

    [Tooltip("The state to enter. Only states every target has are listed.")]
    public string stateName;

    [Tooltip("The next step waits until every target has left this state (or died). Off = the next step starts at once.")]
    public bool waitUntilStateEnds;

    public IEnumerator Run()
    {
        var forced = new List<(Entity entity, object state)>();

        foreach (Entity target in targets)
        {
            if (target == null || target.IsDead) continue;

            if (StateLookup.Enter(target, stateName))
                forced.Add((target, StateLookup.GetState(target, stateName)));
            else
                Debug.LogWarning($"[ForceStateAction] '{target.name}' has no state '{stateName}'.", target);
        }

        if (!waitUntilStateEnds) yield break;

        // Destroyed or dead counts as left, so a target removed some other way can't stall the event.
        while (forced.Exists(f => f.entity != null && !f.entity.IsDead && StateLookup.CurrentState(f.entity) == f.state))
            yield return null;
    }
}
