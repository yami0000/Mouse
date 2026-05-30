using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        CullDestroyedObjects();
        RespawnPersistentObjects();
    }

    // ©¤©¤ Cull permanently destroyed objects ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    private void CullDestroyedObjects()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[SceneController] GameManager not found ¡ª skipping cull.");
            return;
        }

        int culled = 0;
        foreach (var obj in FindObjectsByType<PersistentWorldObject>(FindObjectsSortMode.None))
        {
            if (GameManager.Instance.WasDestroyed(obj.GUID))
            {
                Destroy(obj.gameObject);
                culled++;
            }
        }

        if (culled > 0)
            Debug.Log($"[SceneController] Culled {culled} permanently destroyed object(s).");
    }

    // ©¤©¤ Re-spawn persistent quest objects ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    /// <summary>
    /// On every scene load, checks QuestWorldState for registered spawn GUIDs.
    /// Any that are not currently alive in the scene get re-instantiated at
    /// their QuestSpawnPoint anchor.
    ///
    /// This is the fix for prefabs disappearing after revisiting a scene ¡ª
    /// runtime instances are gone after a scene reload, so we bring them back.
    /// </summary>
    private void RespawnPersistentObjects()
    {
        if (QuestWorldState.Instance == null) return;

        // Build a set of GUIDs currently alive in this scene
        var aliveGUIDs = new System.Collections.Generic.HashSet<string>();
        foreach (var pso in FindObjectsByType<PersistentSpawnObject>(FindObjectsSortMode.None))
            aliveGUIDs.Add(pso.SpawnGUID);

        int respawned = 0;
        foreach (var kv in QuestWorldState.Instance.GetAllSpawnRecords())
        {
            string spawnGUID = kv.Key;
            SpawnRecord record = kv.Value;

            // Already alive in scene ¡ª skip
            if (aliveGUIDs.Contains(spawnGUID)) continue;

            // Find the fixed anchor in this scene
            QuestSpawnPoint anchor = QuestSpawnPoint.Find(record.SpawnPointID);

            // Anchor not in this scene ¡ª this spawn belongs to a different scene, skip
            if (anchor == null) continue;

            // Record already exists in QuestWorldState from the original spawn,
            // so PersistentSpawnObject.Awake() will see WasSpawned() = true.
            Instantiate(record.Prefab, anchor.transform.position, anchor.transform.rotation);
            respawned++;
            Debug.Log($"[SceneController] Respawned '{spawnGUID}' at '{record.SpawnPointID}'.");
        }

        if (respawned > 0)
            Debug.Log($"[SceneController] Respawned {respawned} persistent object(s).");
    }

    // ©¤©¤ Scene Loading ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    public void LoadSceneByIndex(int index) => GameManager.Instance.LoadScene(index);

    public void ToMainCity() => LoadSceneByIndex(1);
    public void ToBugRegion() => LoadSceneByIndex(2);
    public void ToLava() => LoadSceneByIndex(3);
    public void ToBattleField() => LoadSceneByIndex(4);
}
