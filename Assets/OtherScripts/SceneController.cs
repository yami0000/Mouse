 
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
        }
    }

    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadSceneAsync(index);
    }

    // Example wrappers
    public void ToMainCity()
    {
        LoadSceneByIndex(1); // scene 0 in Build Settings
    }

    public void ToBugRegion()
    {
        LoadSceneByIndex(2); // scene 1 in Build Settings
    }

    public void ToLava()
    {
        LoadSceneByIndex(3); // scene 2 in Build Settings
    }

    public void ToBattleField()
    {
        LoadSceneByIndex(4); // scene 2 in Build Settings
    }









}
