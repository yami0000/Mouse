using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Waits before showing a tooltip while the pointer rests on something, and cancels if it leaves first.
/// Every tooltip (items, mods, skills) waits Seconds, so change it here to change them all.
/// Real time, so tooltips still open while a menu pauses the game (Time.timeScale = 0).
/// </summary>
public class HoverDelay
{
    public const float Seconds = 0.5f;

    private readonly MonoBehaviour owner;
    private Coroutine routine;

    public HoverDelay(MonoBehaviour _owner) => owner = _owner;

    /// <summary>Calls _show after Seconds, unless Stop() comes first. Starting again restarts the wait.</summary>
    public void Start(Action _show)
    {
        Stop();

        if (owner.isActiveAndEnabled)
            routine = owner.StartCoroutine(Wait(_show));
    }

    public void Stop()
    {
        if (routine == null) return;

        owner.StopCoroutine(routine);
        routine = null;
    }

    private IEnumerator Wait(Action _show)
    {
        yield return new WaitForSecondsRealtime(Seconds);
        routine = null;
        _show();
    }
}
