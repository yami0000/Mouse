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

    private void Walk() => StartCoroutine(walk());

    IEnumerator walk()
    {
        PlayerManager.Instance.player.isAutoControl = true;

        float t = walktime;



        while (t > 0)
        {
            t -= Time.deltaTime;

            PlayerManager.Instance.player.xInput = Dir;



            yield return null;
        }
        PlayerManager.Instance.player.xInput = 0;

        PlayerManager.Instance.player.isAutoControl = false;


    }

    private void OnEnable() => all.Add(this);
    private void OnDisable() => all.Remove(this);

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
