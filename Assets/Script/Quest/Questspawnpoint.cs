using UnityEngine;

/// <summary>
/// Marker component. Place this on an empty GameObject in any scene
/// to define a fixed spawn location for quest-driven prefabs.
///
/// QuestWorldEvent finds it at runtime by spawnPointID ¡ª no cross-scene
/// dragging required. The ID just needs to match what you type in
/// QuestWorldEvent's Inspector field.
///
/// Example IDs: "MainCity_QuestSpawn_A", "BugRegion_BossArena_Spawn"
/// </summary>
public class QuestSpawnPoint : MonoBehaviour
{
    [Tooltip("Unique ID for this spawn point. Must match the spawnPointID field in QuestWorldEvent.")]
    public string spawnPointID;

    // ©¤©¤ Static Lookup ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    /// <summary>
    /// Finds a QuestSpawnPoint in the current scene by ID.
    /// Returns null and logs a warning if not found.
    /// </summary>
    public static QuestSpawnPoint Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var point in FindObjectsByType<QuestSpawnPoint>(FindObjectsSortMode.None))
        {
            if (point.spawnPointID == id)
                return point;
        }

        Debug.LogWarning($"[QuestSpawnPoint] No spawn point found with ID '{id}' in scene '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'.");
        return null;
    }

    // Draw a visible gizmo in the editor so you can see spawn points
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.8f);
        Gizmos.DrawSphere(transform.position, 0.25f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.6f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f,
            string.IsNullOrEmpty(spawnPointID) ? "(no ID)" : spawnPointID);
#endif
    }
}
