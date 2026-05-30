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

    private void RespawnPersistentObjects()
    {
        if (QuestWorldState.Instance == null)
        {
            Debug.LogWarning("[SceneController] QuestWorldState is null ¡ª skipping respawn.");
            return;
        }

        // Collect GUIDs of PersistentSpawnObjects already alive in this scene
        var aliveGUIDs = new System.Collections.Generic.HashSet<string>();
        foreach (var pso in FindObjectsByType<PersistentSpawnObject>(FindObjectsSortMode.None))
        {
            aliveGUIDs.Add(pso.SpawnGUID);
            Debug.Log($"[SceneController] Alive PersistentSpawnObject found: '{pso.SpawnGUID}'");
        }

        var records = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, SpawnRecord>>(
            QuestWorldState.Instance.GetAllSpawnRecords());

        Debug.Log($"[SceneController] Total spawn records in QuestWorldState: {records.Count}");

        int respawned = 0;
        foreach (var kv in records)
        {
            string spawnGUID = kv.Key;
            SpawnRecord record = kv.Value;

            Debug.Log($"[SceneController] Checking record '{spawnGUID}' | SpawnPointID: '{record.SpawnPointID}' | Prefab: {(record.Prefab == null ? "NULL" : record.Prefab.name)}");

            if (aliveGUIDs.Contains(spawnGUID))
            {
                Debug.Log($"[SceneController] '{spawnGUID}' already alive ¡ª skipping.");
                continue;
            }

            if (record.Prefab == null)
            {
                Debug.LogError($"[SceneController] Cannot respawn '{spawnGUID}' ¡ª prefab is null.");
                continue;
            }

            QuestSpawnPoint anchor = QuestSpawnPoint.Find(record.SpawnPointID);
            if (anchor == null)
            {
                Debug.Log($"[SceneController] No QuestSpawnPoint '{record.SpawnPointID}' in this scene ¡ª belongs elsewhere, skipping.");
                continue;
            }

            Instantiate(record.Prefab, anchor.transform.position, anchor.transform.rotation);
            respawned++;
            Debug.Log($"[SceneController] Respawned '{spawnGUID}' at '{record.SpawnPointID}'.");
        }

        if (respawned > 0)
            Debug.Log($"[SceneController] Respawned {respawned} persistent object(s).");
        else
            Debug.Log($"[SceneController] Nothing to respawn in '{SceneManager.GetActiveScene().name}'.");
    }

    // ©¤©¤ Scene Loading ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    public void LoadSceneByIndex(int index) => GameManager.Instance.LoadScene(index);

    public void ToMainCity() => LoadSceneByIndex(1);
    public void ToBugRegion() => LoadSceneByIndex(2);
    public void ToLava() => LoadSceneByIndex(3);
    public void ToBattleField() => LoadSceneByIndex(4);
}
