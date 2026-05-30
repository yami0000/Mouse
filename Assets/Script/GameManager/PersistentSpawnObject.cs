using UnityEngine;

/// <summary>
/// Attach this to any prefab that is spawned by a QuestWorldEvent.
///
/// On Awake it self-registers into QuestWorldState so the record is always
/// valid regardless of whether QuestWorldEvent or SceneController did the
/// instantiation. This also fixes the execution-order issue where Awake fires
/// inside Instantiate before the caller can call RegisterSpawned.
///
/// SETUP:
///   spawnGUID must exactly match the spawnGUID in the QuestEventEntry
///   that spawns this prefab, AND the spawnPointID of the QuestSpawnPoint
///   in the scene this prefab belongs to.
/// </summary>
public class PersistentSpawnObject : MonoBehaviour
{
    [Tooltip("Must match the spawnGUID in QuestEventEntry and the QuestSpawnPoint.spawnPointID.")]
    [SerializeField] private string spawnGUID;

    [Tooltip("The QuestSpawnPoint ID in this scene. Stored here so SceneController " +
             "can respawn this object without needing QuestWorldEvent to be involved.")]
    [SerializeField] private string spawnPointID;

    public string SpawnGUID => spawnGUID;
    public string SpawnPointID => spawnPointID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(spawnGUID))
        {
            Debug.LogError($"[PersistentSpawnObject] '{gameObject.name}' has no spawnGUID set!", gameObject);
            return;
        }

        if (QuestWorldState.Instance == null) return;

        // Self-register if not already known. This handles both cases:
        // 1. First spawn via QuestWorldEvent ¡ª registers before caller can.
        // 2. Respawn via SceneController ¡ª record already exists, this is a no-op.
        if (!QuestWorldState.Instance.WasSpawned(spawnGUID))
        {
            if (string.IsNullOrEmpty(spawnPointID))
            {
                Debug.LogError($"[PersistentSpawnObject] '{gameObject.name}' has no spawnPointID set! " +
                               "Cannot register for respawn on scene reload.", gameObject);
                return;
            }

            QuestWorldState.Instance.RegisterSpawned(spawnGUID, gameObject, spawnPointID);
            Debug.Log($"[PersistentSpawnObject] Self-registered '{spawnGUID}' at '{spawnPointID}'.");
        }
    }
}
