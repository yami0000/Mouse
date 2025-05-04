 
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

    public void ToBugReagion()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex+1);

    }

    public void ToMainCity()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex - 1);

    }

    public void ToMaze()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void LoadScene(string scenename) 
    {
    SceneManager.LoadSceneAsync(scenename);  
    }











}
