using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public enum WhichSide
{
    Left,
    Right,
 }
 
public class ElevatorDoor : MonoBehaviour
{
   public WhichSide WhichSide;
  
   
   [HideInInspector] public bool isOpened;
   [HideInInspector]public bool isMoving = false;


    [SerializeField]private Auto_Elevator Elevator;
    [SerializeField]private Auto_Elevator AnotherElevator;
    [SerializeField]private ElevatorPlatform Platform;  

    [SerializeField] private float distance ;      // How far left it moves
   [SerializeField] private float smoothTime;  // Movement speed/acceleration

    private Vector2 closedPos;
    private Vector2 openPos;
    private Vector2 velocity = Vector2.zero;

    private void Start()
    {
        closedPos = transform.position;
        if (WhichSide == WhichSide.Left)
            openPos = closedPos + Vector2.left * distance;
        if (WhichSide == WhichSide.Right)
            openPos = closedPos + Vector2.right * distance;
    }
    private void Update()
    {


        if (Elevator.Interact || AnotherElevator.Interact)
            return;
        
        if (Elevator.ElevatorReadyToOpen && !isMoving )
        {
            if(Elevator.WhichFloor == WhichFloor.First && Platform.WhichFloor == 1 || Elevator.WhichFloor == WhichFloor.Second && Platform.WhichFloor == 2)
            Open();
            else return;
             

        }
        if (!Elevator.ElevatorReadyToOpen && isOpened && !isMoving)
        {
            Close();
             
        }

    }

    public void Open()
    {
        if (!isMoving)
            StartCoroutine(MoveDoor());
    }

    public void Close()
    {
        if (!isMoving)
            StartCoroutine(CloseDoor());
    }

    public IEnumerator MoveDoor() 
    {
        isMoving = true;
        velocity = Vector2.zero;

        while (Vector2.Distance(transform.position, openPos) > 0.01f)
        {
            transform.position = Vector2.SmoothDamp(transform.position, openPos, ref velocity, smoothTime);
            yield return null;
        }
        //transform.position = openPos;
        isOpened = true;
        isMoving = false;
    }

    public IEnumerator CloseDoor()
    {
        isMoving = true;
        velocity = Vector2.zero;

        while (Vector2.Distance(transform.position, closedPos) > 0.01f)
        {
            transform.position = Vector2.SmoothDamp(transform.position, closedPos, ref velocity, smoothTime);
            yield return null;
        }
        //transform.position = closedPos;
        isOpened = false;
        isMoving = false;
    }


}
