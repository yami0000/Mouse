using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DETECTION : MonoBehaviour
{


    [HideInInspector]public bool IsReadyToInteract = false;
  
    private GameObject e;

    
    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
           IsReadyToInteract = true;
            if(e == null)e = Instantiate(GetPlayer().E, GetPlayer().transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Player>(out Player player))
        {
            IsReadyToInteract = false;
            if(e!=null)Destroy(e);
        }
    }

    private void Update()
    {
        Vector3 V= new Vector3(GetPlayer().transform.position.x,
                              GetPlayer().transform.position.y+2.5f,
                              GetPlayer().transform.position.z);

        if(e!=null)e.transform.position =V;  
          
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
