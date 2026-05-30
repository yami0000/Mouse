using UnityEngine;

public class PersistentWorldObject : MonoBehaviour
{
    [SerializeField] private string guid; // set manually in the Inspector ¡ª must be globally unique

    /// <summary>Exposes the GUID so SceneController can read it during culling.</summary>
    public string GUID => guid;

    private void Awake()
    {
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError($"[PersistentWorldObject] '{gameObject.name}' has no GUID set! " +
                           "Assign a unique ID in the Inspector.", gameObject);
            return;
        }

        // Awake-time check: cull immediately if already recorded as destroyed.
        // This is a safety net ¡ª SceneController.CullDestroyedObjects() handles
        // the bulk pass, but Awake catches objects spawned after scene load too.
        if (GameManager.Instance != null && GameManager.Instance.WasDestroyed(guid))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Permanently destroys this object. It will never appear again in any scene
    /// or session as long as GameManager state persists.
    /// </summary>
    public void DestroyPersistently()
    {
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError($"[PersistentWorldObject] Cannot permanently destroy '{gameObject.name}': no GUID set.");
            return;
        }

        GameManager.Instance.RecordDestroyed(guid);
        Destroy(gameObject);
    }
}
