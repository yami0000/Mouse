using UnityEngine;

/// <summary>
/// Attach this to any prefab that is spawned by a QuestWorldEvent.
///
/// When the prefab is instantiated, this component's Awake() confirms
/// the spawnGUID is registered in QuestWorldState. On scene reload,
/// SceneController finds all registered GUIDs, checks which are not
/// currently alive in the scene, and re-instantiates them using
/// QuestSpawnPoint.Find(spawnPointID).
///
/// SETUP:
///   - spawnGUID must exactly match the spawnGUID in the QuestEventEntry
///     that spawns this prefab.
///   - No Transform or scene reference needed ¡ª the spawnPointID is
///     stored in QuestWorldState.SpawnRecord and looked up at runtime.
/// </summary>
public class PersistentSpawnObject : MonoBehaviour
{
    [Tooltip("Must match the spawnGUID in the QuestEventEntry that spawns this prefab.")]
    [SerializeField] private string spawnGUID;

    public string SpawnGUID => spawnGUID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(spawnGUID))
        {
            Debug.LogError($"[PersistentSpawnObject] '{gameObject.name}' has no spawnGUID set!", gameObject);
            return;
        }

        // Safety confirmation ¡ª if somehow this instance exists but isn't
        // registered yet (e.g. manually placed in scene), register it now.
        if (QuestWorldState.Instance != null && !QuestWorldState.Instance.WasSpawned(spawnGUID))
        {
            Debug.LogWarning($"[PersistentSpawnObject] '{spawnGUID}' was not registered in QuestWorldState. " +
                             "Was this prefab placed manually instead of spawned by QuestWorldEvent?");
        }
    }
}
