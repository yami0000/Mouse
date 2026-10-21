using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorFS : MonoBehaviour

{
    [SerializeField] private ElevatorDetectionFS detection;
     
    [SerializeField] private Transform A;
    [SerializeField] private Transform B;

    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float stopDistance = 0.05f;

    private bool isMoving = false;

    private void Update()
    {
        
       
        if (isMoving) return;
        
        if(detection.IsReadyToInteract && detection.CanReverse)
        {  
            
            Vector2 targetPos =
            Vector2.Distance(transform.position, A.position) <
            Vector2.Distance(transform.position, B.position)
            ? B.position
            : A.position;
        
        StartCoroutine(MoveElevator(targetPos));
        
        }

       
     
    }

     

        

    private System.Collections.IEnumerator MoveElevator(Vector2 target)
    {
        isMoving = true;
        float speed = 0f;

        while (true)
        {
            Vector2 pos = transform.position;
            Vector2 dir = (target - pos).normalized;
            float dist = Vector2.Distance(pos, target);

             
            float decelFactor = Mathf.Clamp01(dist / 1f);  
            float targetSpeed = maxSpeed * decelFactor;

           
            speed = Mathf.MoveTowards(speed, targetSpeed, acceleration * Time.deltaTime);

             
            Vector2 move = dir * speed * Time.deltaTime;

            if (dist <= stopDistance)
                break;

            transform.position = pos + move;

            yield return null;
        }

        transform.position = target;
        detection. CanReverse = false;
        isMoving = false;
    }

}
