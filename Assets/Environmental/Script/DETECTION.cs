using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DETECTION : MonoBehaviour
{


    [HideInInspector]public bool IsReadyToInteract = false;
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
        }
    }

    private void Update()
    {
        if (IsReadyToInteract)
        {
                Event();
            if (Input.GetKeyDown(KeyCode.E))
                Interact();
        }
        
    }

    public virtual void Interact() 
    {


    }

    public virtual void Event() 
    {
    
    }
}
