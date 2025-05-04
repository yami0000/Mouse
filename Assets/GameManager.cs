using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMonobehaviour<GameManager>
{
    [SerializeField] private int Num; 
    private void Start()
    {
        SceneManager.LoadSceneAsync(Num);

        // SceneController.Instance.LoadScene("Scene name");
    }



}
