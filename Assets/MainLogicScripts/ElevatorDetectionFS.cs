using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDetectionFS : MonoBehaviour
{

    public bool IsReadyToInteract;
    public bool CanReverse;

    private void Start()
    {
        CanReverse = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
            IsReadyToInteract = true;
           
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
            IsReadyToInteract = false;
            CanReverse = true;
        }
    }
}
