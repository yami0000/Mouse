using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class stalactite : MonoBehaviour
{
    [SerializeField] protected Transform playerCheck;
    [SerializeField] protected float playerCheckDistance;
    [SerializeField] protected float groundCheckDistance;
    [SerializeField] LayerMask whatIsPlayer;
    [SerializeField] LayerMask whatIsGround;
    private EntityStats EntityStats;
    private Rigidbody2D rb;
    private bool istriggered = false;

    private EntityStats stats => GetComponentInParent<EntityStats>();
    public bool IsPlayerDetected() => Physics2D.Raycast(playerCheck.position, Vector2.down, playerCheckDistance, whatIsPlayer);
    public bool IsGroundDetected() => Physics2D.Raycast(playerCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    private void Start()
    {

        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        if (IsPlayerDetected()) 
        {
            rb.gravityScale = 5f;
            istriggered = true;
        }
        if(istriggered)
            if (IsGroundDetected())
                Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {



        if (collision.GetComponent<Player>() != null)
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();


            stats.DoDamage(player);
            

            Destroy(gameObject);
        }

        
    }

 
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x, playerCheck.position.y - playerCheckDistance));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x, playerCheck.position.y - groundCheckDistance));
    }
}
