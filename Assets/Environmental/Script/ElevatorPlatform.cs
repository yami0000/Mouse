using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{

    public BoxCollider2D cd { get; private set; }

    [Header("Elevator/Door")]
    [SerializeField] private Auto_Elevator Elevator1F;
    [SerializeField] private Auto_Elevator Elevator2F;
    //[HideInInspector] public bool  
    [SerializeField] private ElevatorDoor Left1F;
    [SerializeField] private ElevatorDoor Right1F;
    [SerializeField] private ElevatorDoor Left2F;
    [SerializeField] private ElevatorDoor Right2F;

    [Header("Position of 1F/2F")]
    [SerializeField] private Transform POS2F;

    private Vector2 Pos1F;
    private Vector2 Pos2F;
    private SpriteRenderer sr;
    private Vector2 velocity = Vector2.zero;
    private bool elevatorIsMoving = false;
    private bool notOnSameLevel;

    public int WhichFloor = 1;

    [Header("Acceleration")]
    [SerializeField] private float smoothTime;
    [SerializeField] private float moveSpeed;

    [SerializeField]private ElevatorPlatform platform;
    private void Awake()
    {
        sr = PlayerManager.Instance.player.GetComponentInChildren<SpriteRenderer>();
        cd = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        Pos1F = transform.position;
        Pos2F = POS2F.position;
    }

    private void Update()
    {

        if (Elevator1F.Interact && !elevatorIsMoving )
        {
            
            if (WhichFloor == 2)
            {
                 
                cd.enabled = false;
                StartCoroutine(GoingDown());
               
                
            }
            else if(Left1F.isOpened)
            {
                
                GM.Instance.GameManager.isInteract = true;
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 8;
                StartCoroutine(GoingUp());

            }
        }
        if (Elevator2F.Interact && !elevatorIsMoving )
        {
            
            if (WhichFloor == 1)
            {   
                StartCoroutine(GoingUp());
                 
            }
            else if ( Left2F.isOpened)
            {
                
                GM.Instance.GameManager.isInteract = true;
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 8;
                StartCoroutine(GoingDown());

            }
        }

        IEnumerator GoingUp()
        {

            elevatorIsMoving = true;


            Left1F.Close();
            Right1F.Close();

            while (Left1F.isMoving || Right1F.isMoving)
            {
                yield return null;
            }


            while (Vector2.Distance(transform.position, Pos2F) > 0.1f)
            {
                transform.position = Vector2.SmoothDamp(transform.position, Pos2F, ref velocity, smoothTime);
                yield return null;
            }

            Left2F.Open();
            Right2F.Open();
            while (Left2F.isMoving || Right2F.isMoving)
            {
                yield return null;
            }

            sr.sortingLayerName = "Player";
            sr.sortingOrder = 11;

            WhichFloor = 2;
            GM.Instance.GameManager.isInteract = false;
            Elevator1F.Interact = false;
            Elevator2F.Interact = false;
            elevatorIsMoving = false;
        }
        IEnumerator GoingDown()
        {
            

            elevatorIsMoving = true;

            Left2F.Close();
            Right2F.Close();

            while (Left2F.isMoving || Right2F.isMoving)
            {
                yield return null;
            }

            while (Vector2.Distance(transform.position, Pos1F) > 0.1f)
            {
                transform.position = Vector2.SmoothDamp(transform.position, Pos1F, ref velocity, smoothTime);
                yield return null;
            }

            Left1F.Open();
            Right1F.Open();
            while (Left1F.isMoving || Right1F.isMoving)
            {
                yield return null;
            }
            sr.sortingLayerName = "Player";
            sr.sortingOrder = 11;

            WhichFloor = 1;
            GM.Instance.GameManager.isInteract = false;
            Elevator2F.Interact = false;
            Elevator1F.Interact = false;
            elevatorIsMoving = false;
            cd.enabled = true;
        }
    }
}