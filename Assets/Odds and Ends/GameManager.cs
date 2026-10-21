using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn;
using Yarn.Unity;

[System.Serializable]
public class GameState
{
    // Quest state
    public List<string> completedQuests = new();
    public List<string> completedObjectives = new();
    public List<string> activeQuests = new();

    // World state
    public List<string> destroyedObjects = new();    // by GUID
    public List<string> spawnedObjects = new();      // by GUID
    public Dictionary<string, bool> worldFlags = new(); // general purpose booleans

    // Player state
    public Vector3 playerPosition;
    public string currentScene;
    public List<string> inventory = new();
}

public class GameManager : SingletonMonobehaviour<GameManager>
{
    public GameState State { get; private set; } = new();

    public string ID;
    private DialogueRunner DialogueRunner;

    [HideInInspector] public bool isUIOpened;
    [HideInInspector] public bool isInteract;
    [HideInInspector] public bool isInteractGrandma;
 
    private Animator ani;

 

    public string PendingSpawnId { get; private set; }
    private bool isLoadingScene;   // AutoLoad doors call LoadScene every frame; ignore repeats until the scene has loaded
    private void Awake()
    {
        // Ensure GameManager persists across all scene loads
        if (FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Fires every time a new scene finishes loading.
    /// Culls permanently destroyed objects and respawns persistent quest objects.
    /// </summary>
    /// 

    public void LoadScene(int index, string spawnId = null)
    {
        if (isLoadingScene) return;
        isLoadingScene = true;
        PendingSpawnId = spawnId;
        State.currentScene = index.ToString();
        SceneManager.LoadSceneAsync(index);
    }

    // Load the scene whose build index is the AA part of an "AA-BB" spawn ID.
    public void LoadScene(string spawnId)
    {
        if (!TryGetSceneIndex(spawnId, out int index))
        {
            Debug.LogError($"[GameManager] Spawn ID '{spawnId}' is not in the AA-BB format.");
            return;
        }
        LoadScene(index, spawnId);
    }

    // "02-03" -> 2, "10-01" -> 10
    public static bool TryGetSceneIndex(string spawnId, out int index)
    {
        index = -1;
        if (string.IsNullOrEmpty(spawnId)) return false;
        int dash = spawnId.IndexOf('-');
        return dash > 0 && int.TryParse(spawnId.Substring(0, dash), out index);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoadingScene = false;
        CullDestroyedObjects();
        RespawnPersistentObjects();
        PlacePlayerAtPendingSpawn();
    }

    private void PlacePlayerAtPendingSpawn()
    {
        if (string.IsNullOrEmpty(PendingSpawnId)) return;

        SpawnPoint sp = SpawnPoint.Find(PendingSpawnId);
        if (sp == null)
        {
            Debug.LogWarning($"[GameManager] No SpawnPoint with id '{PendingSpawnId}' in this scene.");
            PendingSpawnId = null;
            return;
        }

            PlayerManager.Instance.player.transform.position = sp.transform.position;

            Player p = PlayerManager.Instance.player;
            if (p != null && p.facingDir != sp.Dir)
                p.Flip();
         

        PendingSpawnId = null;    
    }
    private void CullDestroyedObjects()
    {
        int culled = 0;
        foreach (var obj in FindObjectsByType<PersistentWorldObject>(FindObjectsSortMode.None))
        {
            if (WasDestroyed(obj.GUID))
            {
                Destroy(obj.gameObject);
                culled++;
            }
        }

        if (culled > 0)
            Debug.Log($"[GameManager] Culled {culled} permanently destroyed object(s).");
    }

    private void RespawnPersistentObjects()
    {
        if (QuestWorldState.Instance == null) return;

        Debug.Log("ReSpawned");

        var aliveGUIDs = new HashSet<string>();
        foreach (var pso in FindObjectsByType<PersistentSpawnObject>(FindObjectsSortMode.None))
            aliveGUIDs.Add(pso.SpawnGUID);

        int respawned = 0;
        foreach (var kv in QuestWorldState.Instance.GetAllSpawnRecords())
        {
            string spawnGUID = kv.Key;
            SpawnRecord record = kv.Value;

            if (aliveGUIDs.Contains(spawnGUID)) continue;

            if (record.Prefab == null)
            {
                Debug.LogError($"[GameManager] Cannot respawn '{spawnGUID}' ¡ª prefab is null.");
                continue;
            }

            QuestSpawnPoint anchor = QuestSpawnPoint.Find(record.SpawnPointID);
            if (anchor == null) continue;

            Instantiate(record.Prefab, anchor.transform.position, anchor.transform.rotation);
            respawned++;
            Debug.Log($"[GameManager] Respawned '{spawnGUID}' at '{record.SpawnPointID}'.");
        }

        if (respawned > 0)
            Debug.Log($"[GameManager] Respawned {respawned} persistent object(s).");
    }

    public Vector2 GetMouse(bool restrict)
    {
        if (PlayerManager.Instance.player != null)
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            Vector2 direction = (mouseWorldPos - PlayerManager.Instance.player.transform.position).normalized;

            direction.x = PlayerManager.Instance.player.facingDir == 1
                ? Mathf.Max(direction.x, 0.1f)
                : Mathf.Min(direction.x, -0.1f);

            if (restrict)
            {
                float ratio60Degrees = 1.732f;
                float limit = Mathf.Abs(direction.x) * ratio60Degrees;
                direction.y = Mathf.Clamp(direction.y, -limit, limit);
            }

            return direction.normalized;
        }
        else
            return Vector2.zero;
    }

    private void Start()
    {
        
        LoadScene(ID);
        ani = PlayerManager.Instance.player.GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        DialogueRunner = FindObjectOfType<DialogueRunner>();

        if (DialogueRunner != null && DialogueRunner.IsDialogueRunning || isUIOpened || isInteract)
        {
            Player playerScript = FindObjectOfType<Player>();
            playerScript.enabled = false;
        }
        else
        {
            Player playerScript = FindObjectOfType<Player>();
            playerScript.enabled = true;
        }
    }

    // ©¤©¤ Quest State ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    public void RecordObjectiveCompleted(string key)
    {
        if (!State.completedObjectives.Contains(key))
            State.completedObjectives.Add(key);
    }

    public void RecordQuestCompleted(string id)
    {
        if (!State.completedQuests.Contains(id))
            State.completedQuests.Add(id);
        State.activeQuests.Remove(id);
    }

    public void RecordQuestAccepted(string id)
    {
        if (!State.activeQuests.Contains(id))
            State.activeQuests.Add(id);
    }

    public bool IsObjectiveCompleted(string key) => State.completedObjectives.Contains(key);
    public bool IsQuestCompleted(string id) => State.completedQuests.Contains(id);
    public bool IsQuestActive(string id) => State.activeQuests.Contains(id);

    // ©¤©¤ World Flags ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    public void SetFlag(string flag, bool value) => State.worldFlags[flag] = value;
    public bool GetFlag(string flag) => State.worldFlags.TryGetValue(flag, out bool v) && v;

    // ©¤©¤ World Objects ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    public void RecordDestroyed(string guid)
    {
        if (!State.destroyedObjects.Contains(guid))
            State.destroyedObjects.Add(guid);
    }

    public void RecordSpawned(string guid)
    {
        if (!State.spawnedObjects.Contains(guid))
            State.spawnedObjects.Add(guid);
    }

    public bool WasDestroyed(string guid) => State.destroyedObjects.Contains(guid);
    public bool WasSpawned(string guid) => State.spawnedObjects.Contains(guid);

    // ©¤©¤ Scene Loading ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    /// <summary>
    /// Load a scene by index. SceneController should call this so GameManager
    /// stays in the loop (future: pre-cull lists, save state before transition, etc.)
    /// </summary>
    public void LoadScene(int index)
    {
        if (isLoadingScene) return;
        isLoadingScene = true;
        State.currentScene = index.ToString();
        SceneManager.LoadSceneAsync(index);
    }
}
