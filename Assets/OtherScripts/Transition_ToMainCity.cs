using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Transition_ToMainCity : MonoBehaviour
{
    public Transform Center;
    
     
    public int Radius;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        player.transform.position = Center.position;
    }

    private void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Center.transform.position, Radius);
        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Player>() != null && Input.GetKey(KeyCode.E))
            {

                SceneController.instance.ToMaze();

            }
        }
    }
    protected virtual void OnDrawGizmos()
    { 
        Gizmos.DrawWireSphere(Center.transform.position, Radius);
    }
}
