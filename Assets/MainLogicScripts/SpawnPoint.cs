using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : DETECTION
{
    public string id;                 // e.g. "FromMainCity", "FromMaze_BackDoor"
    [Tooltip("Rigel will walk automatically when entered")]
    public bool _walk;
    [Tooltip("How long will character walk")]
    public float walktime;
    [Tooltip("Direction")]
    public float Dir;

    private bool IfWalked = false;

    private static readonly List<SpawnPoint> all = new();

    private void Start()
    {
       
    }

    private Coroutine walkRoutine;

    private void Walk() => walkRoutine = StartCoroutine(walk());

    IEnumerator walk()
    {
        Player p = PlayerManager.Instance.player;
        if (p == null) yield break;
        try
        {
            p.isAutoControl = true;
            float t = walktime;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                p.xInput = Dir;
                yield return null;
            }
        }
        finally   // runs on normal end AND when the coroutine is stopped/disposed
        {
            p.xInput = 0f;
            p.isAutoControl = false;
            walkRoutine = null;
        }
    }

    private void OnDisable()
    {
        all.Remove(this);
        if (walkRoutine != null)        // we were driving the player and got disabled
        {
            StopCoroutine(walkRoutine);
            walkRoutine = null;
            Player p = PlayerManager.Instance != null ? PlayerManager.Instance.player : null;
            if (p != null) { p.xInput = 0f; p.isAutoControl = false; }
        }
    }

    private void OnEnable() => all.Add(this);
   

    public static SpawnPoint Find(string id)
    {
        foreach (var s in all)
            if (s.id == id) return s;
        return null;
    }

    public override void Event()
    {

        base.Event();
        
            if (!IfWalked && _walk && IsReadyToInteract)
            { 
                Walk();
                IfWalked = true;
            }
    }
}
