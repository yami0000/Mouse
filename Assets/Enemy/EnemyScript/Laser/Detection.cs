using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    [SerializeField] float range;
    [SerializeField] LayerMask PlayerDetection;

    public RaycastHit2D hit;
    public Vector2 Hitposition => hit.point;

    private void Update()
    {
        //IsPlayerInRange();
     
    }
    public bool IsPlayerInRange()
    {
        float distance = (PlayerManager.Instance.player.transform.position - transform.position).magnitude;

        if (distance < range)
        {
             
            return true; 
        }
        else
        {
            
            return false;
        }
         

    }

    private void OnDrawGizmos() 
    {
         

        if (hit)
        {   Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hit.point, .3f); 
        }

    }

    public bool HasLineOfSightOfWall() 
    {
        hit =  Physics2D.Raycast(transform.position, DirectionToPlayer(),range,PlayerDetection);
        if (hit)
         return false;
        else 
         return true;
        
        
        

     }

    public Vector2 DirectionToPlayer()
    {
      return (PlayerManager.Instance.player.transform.position-transform.position ).normalized;
    
    }
}
