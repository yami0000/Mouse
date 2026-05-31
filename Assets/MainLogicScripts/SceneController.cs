using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Per-scene navigation helper. Just routes scene transitions through GameManager.
/// Cull and respawn are handled by GameManager.OnSceneLoaded ¡ª no per-scene setup needed.
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void LoadSceneByIndex(int index) => GameManager.Instance.LoadScene(index);

    public void ToMainCity() => LoadSceneByIndex(1);
    public void ToBugRegion() => LoadSceneByIndex(2);
    public void ToLava() => LoadSceneByIndex(3);
    public void ToBattleField() => LoadSceneByIndex(4);
}
