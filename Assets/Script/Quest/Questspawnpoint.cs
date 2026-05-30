using UnityEngine;

/// <summary>
/// Fixed spawn anchor. Place one on an empty GameObject in each scene
/// where a quest event needs to instantiate a prefab.
///
/// QuestWorldEvent holds a direct reference to this in the Inspector.
/// QuestWorldState.SpawnRecord stores the spawnPointID string.
/// SceneController uses QuestSpawnPoint.Find(spawnPointID) on scene reload
/// to locate the anchor and re-instantiate persistent spawns.
///
/// Example IDs: "MainCity_QuestSpawn_A", "BugRegion_BossArena_Spawn"
/// </summary>
public class QuestSpawnPoint : MonoBehaviour
{
    [Tooltip("Unique ID for this anchor. Defaults to the GameObject name if left empty.")]
    public string spawnPointID;

    private void Awake()
    {
        // Auto-assign from GameObject name if not set ¡ª prevents silent empty-ID bugs
        if (string.IsNullOrEmpty(spawnPointID))
        {
            spawnPointID = gameObject.name;
            Debug.LogWarning($"[QuestSpawnPoint] No ID set ¡ª using GameObject name '{spawnPointID}'. " +
                             "Set a proper ID in the Inspector to avoid name-change bugs.");
        }
    }

    // ©¤©¤ Static Lookup ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    /// <summary>
    /// Finds a QuestSpawnPoint in the current scene by ID.
    /// Returns null and logs a warning if not found.
    /// </summary>
    public static QuestSpawnPoint Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var point in FindObjectsByType<QuestSpawnPoint>(FindObjectsSortMode.None))
            if (point.spawnPointID == id)
                return point;

        Debug.LogWarning($"[QuestSpawnPoint] No spawn point with ID '{id}' found in scene " +
                         $"'{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'.");
        return null;
    }

    // ©¤©¤ Gizmos ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.8f);
        Gizmos.DrawSphere(transform.position, 0.25f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.6f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.75f,
            string.IsNullOrEmpty(spawnPointID) ? "(no ID)" : spawnPointID);
#endif
    }
}
