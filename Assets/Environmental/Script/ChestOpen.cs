using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChestOpen : DETECTION
{
    private ItemDrop dropSystem;

    [SerializeField]private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D ColliderForBody;
    private Vector2 velocity = Vector2.zero;

    [SerializeField] private Vector2 upOffset  ;
    [SerializeField] private Vector2 rightOffset ;
    private bool CanInteract;
    [SerializeField] private float RightSmoothTime;
    private void Awake()
    {
        ColliderForBody.enabled = false;
    }

    private void Start()
    {

        dropSystem = GetComponent<ItemDrop>();  

        rb.gravityScale =0;
        CanInteract = true;
    }
    public override void Interact()
    {
        base.Interact();
        if (CanInteract)
        {
            StartCoroutine(MoveSequence());
            CanInteract = false;
        }
    }



    IEnumerator MoveSequence()
    {
         
        Vector2 targetPos = (Vector2)transform.position + upOffset;
        yield return StartCoroutine(MoveToPosition(targetPos, 0.5f));

        
        targetPos = (Vector2)transform.position + rightOffset;
        yield return StartCoroutine(MoveToPosition(targetPos, RightSmoothTime));
        ColliderForBody.enabled = true;
        rb.gravityScale = 1;

        yield return new WaitForSeconds(0.2f);
        dropSystem.GenerateDrop();
    }

    IEnumerator MoveToPosition(Vector2 target, float smoothTime)
    {
        velocity = Vector2.zero; 

        while (Vector2.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector2.SmoothDamp(
                transform.position,
                target,
                ref velocity,
                smoothTime
            );
            yield return null;
        }

       // transform.position = target;  
    }

}
