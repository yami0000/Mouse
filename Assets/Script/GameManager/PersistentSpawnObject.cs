using UnityEngine;

/// <summary>
/// Attach this to any prefab spawned by QuestWorldEvent.
/// Its only job is to be findable by SceneController so it can check
/// which GUIDs are currently alive in the scene.
///
/// Registration into QuestWorldState is handled exclusively by
/// QuestWorldEvent.Spawn() BEFORE Instantiate is called, using the
/// prefab asset reference ¡ª never from a live instance.
/// </summary>
public class PersistentSpawnObject : MonoBehaviour
{
    [Tooltip("Must match the spawnGUID in the QuestEventEntry that spawns this prefab.")]
    [SerializeField] private string spawnGUID;

    public string SpawnGUID => spawnGUID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(spawnGUID))
            Debug.LogError($"[PersistentSpawnObject] '{gameObject.name}' has no spawnGUID set!", gameObject);
    }
}
