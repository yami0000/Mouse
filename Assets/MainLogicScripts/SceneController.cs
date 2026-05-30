using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Every time a scene loads, immediately destroy any objects
        // that were permanently destroyed in a previous session.
        CullDestroyedObjects();
    }

    // ©¤©¤ Persistent Object Culling ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    /// <summary>
    /// Finds every PersistentWorldObject in the scene and destroys any
    /// whose GUID is recorded as destroyed in GameManager.
    /// Called automatically on Start so you never see a "dead" object flicker.
    /// </summary>
    private void CullDestroyedObjects()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[SceneController] GameManager not found ¡ª skipping persistent object cull.");
            return;
        }

        PersistentWorldObject[] allObjects =
            FindObjectsByType<PersistentWorldObject>(FindObjectsSortMode.None);

        int culled = 0;
        foreach (PersistentWorldObject obj in allObjects)
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

    // ©¤©¤ Scene Loading ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤

    public void LoadSceneByIndex(int index)
    {
        GameManager.Instance.LoadScene(index);
    }

    public void ToMainCity() => LoadSceneByIndex(1);
    public void ToBugRegion() => LoadSceneByIndex(2);
    public void ToLava() => LoadSceneByIndex(3);
    public void ToBattleField() => LoadSceneByIndex(4);
}
