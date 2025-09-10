using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WhichFloor
{
    First,
    Second,

}
public class Auto_Elevator : MonoBehaviour
{
    public WhichFloor WhichFloor;
    [HideInInspector] public bool ElevatorReadyToOpen = false;
    [HideInInspector] public bool Interact = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
          ElevatorReadyToOpen = true;   
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
          ElevatorReadyToOpen = false;  
        }
    }
    private void Update()
    {   
        if(ElevatorReadyToOpen)
        if(Input.GetKeyDown(KeyCode.E))
        {
            if (!Interact)
                Interact = true;
           
        }
    }
}

