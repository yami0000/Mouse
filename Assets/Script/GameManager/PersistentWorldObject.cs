using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentWorldObject : MonoBehaviour
{
    [SerializeField] private string guid; // set manually or auto-generate

    void Start()
    {
        // If this object was destroyed in a previous session, destroy it again
        if (GameManager.Instance.WasDestroyed(guid))
        {
            Destroy(gameObject);
            return;
        }
    }

    public void DestroyPersistently()
    {
        GameManager.Instance.RecordDestroyed(guid);
        Destroy(gameObject);
    }
}