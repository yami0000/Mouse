using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn;
using Yarn.Unity;

public class GameManager : SingletonMonobehaviour<GameManager>
{
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

    public Vector2 GetMouse() 
    {
        if (PlayerManager.Instance.player != null)
        {

            Vector3 mouseScreenPos = Input.mousePosition;

            mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            Vector2 direction = (mouseWorldPos - PlayerManager.Instance.player.transform.position).normalized;



            direction.x =PlayerManager.Instance.player.facingDir == 1 ? Mathf.Max(direction.x, 0.1f) : Mathf.Min(direction.x, -0.1f);

            float ratio60Degrees = 1.732f;
            float limit = Mathf.Abs(direction.x)* ratio60Degrees;
            direction.y = Mathf.Clamp(direction.y, -limit, limit);

            
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

}
