using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn;
using Yarn.Unity;

public class GameManager : SingletonMonobehaviour<GameManager>
{
    [SerializeField] private int Num;
    private Rigidbody2D rb;
    private DialogueRunner DialogueRunner;
    [HideInInspector]public bool isUIOpened;
    [HideInInspector] public bool isInteract;
    public bool isMantisBossFightStarted;
        
    private Animator ani;


    [HideInInspector] public int MantisHealth;
    [HideInInspector] public int MantisMaxHealth;
    private void Start()
    {
       
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

}
