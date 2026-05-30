 
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

     
    public void ToMainCity()  =>  LoadSceneByIndex(1);  
    public void ToBugRegion() => LoadSceneByIndex(2);  
    public void ToLava() => LoadSceneByIndex(3);  
    public void ToBattleField() => LoadSceneByIndex(4);  
  








}
