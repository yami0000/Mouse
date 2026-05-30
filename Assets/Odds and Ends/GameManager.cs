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
    [SerializeField] private int Num;
   // private Rigidbody2D rb;
    private DialogueRunner DialogueRunner;
    [HideInInspector]public bool isUIOpened;
    [HideInInspector] public bool isInteract;
    [HideInInspector] public bool isInteractGrandma;
    public bool isMantisBossFightStarted;
    public bool isMantisAlive;

    private Animator ani;

    [HideInInspector] public int MantisHealth;
    [HideInInspector] public int MantisMaxHealth;
    [HideInInspector] public int ScorpionHealth;
    [HideInInspector] public int ScorpionMaxHealth;

    public Vector2 GetMouse(bool restrict) 
    {
        if (PlayerManager.Instance.player != null)
        {

            Vector3 mouseScreenPos = Input.mousePosition;

            mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            Vector2 direction = (mouseWorldPos - PlayerManager.Instance.player.transform.position).normalized;



            direction.x =PlayerManager.Instance.player.facingDir == 1 ? Mathf.Max(direction.x, 0.1f) : Mathf.Min(direction.x, -0.1f);

            if (restrict)
            {
                float ratio60Degrees = 1.732f;
                float limit = Mathf.Abs(direction.x) * ratio60Degrees;
                direction.y = Mathf.Clamp(direction.y, -limit, limit);
            }

            
            direction = direction.normalized;



            return direction;
        }
        else 
            return Vector2.zero;
    }
    private void Start()
    {
       
        isMantisAlive = true;
        SceneManager.LoadSceneAsync(Num);
        ani = PlayerManager.Instance.player.GetComponentInChildren<Animator>();
        // SceneController.Instance.LoadScene("Scene name");

    }

    private void Update()
    {
        


        DialogueRunner = FindObjectOfType<DialogueRunner>();

         
        
           if (DialogueRunner != null && DialogueRunner.IsDialogueRunning || isUIOpened || isInteract)
            {

                Player playerScript = FindObjectOfType<Player>();
                playerScript.enabled = false;
                //ani.enabled = false ;    
                 
            }
            else
            {
                Player playerScript = FindObjectOfType<Player>();
                playerScript.enabled = true;
                //ani.enabled = true;
        }
         
    }

    // ©¤©¤ Quest State ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

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

    // ©¤©¤ World Flags ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
    // General purpose ¡ª use for anything that doesn't fit elsewhere
    // e.g. "met_elder", "bridge_destroyed", "dungeon_unlocked"

    public void SetFlag(string flag, bool value) => State.worldFlags[flag] = value;
    public bool GetFlag(string flag) => State.worldFlags.TryGetValue(flag, out bool v) && v;

    // ©¤©¤ World Objects ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

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

   
}


