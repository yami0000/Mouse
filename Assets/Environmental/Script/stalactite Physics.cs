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
    private Wiggle wiggle;
    private bool istriggered = false;

    private EntityStats stats => GetComponentInParent<EntityStats>();
    public bool IsPlayerDetected() => Physics2D.Raycast(playerCheck.position, Vector2.down, playerCheckDistance, whatIsPlayer);
    public bool IsGroundDetected() => Physics2D.Raycast(playerCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    private void Start()
    {

        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        wiggle = GetComponent<Wiggle>();
    }

    private void Update()
    {
        if (!istriggered && IsPlayerDetected())
        {
            // stop wiggling, otherwise it keeps resetting the position and the stalactite can't fall
            if (wiggle != null)
                wiggle.enabled = false;

            rb.gravityScale = 5f;
            istriggered = true;
        }
        if(istriggered)
            if (IsGroundDetected())
                DestroySelf();
    }

    private void DestroySelf()
    {
        // let child particles fade out instead of vanishing with the stalactite
        foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
        {
            // nested systems are handled by their top particle system
            if (ps.transform.parent.GetComponentInParent<ParticleSystem>() != null)
                continue;

            float lifetime = 0f;
            foreach (ParticleSystem sub in ps.GetComponentsInChildren<ParticleSystem>())
                lifetime = Mathf.Max(lifetime, sub.main.startLifetime.constantMax);

            // with Local scaling the parent's scale was ignored, so keep the same local scale after detaching
            // (the stalactite has a flipped Y scale, which would otherwise flip and speed up the particles)
            Vector3 scale = ps.transform.localScale;
            ps.transform.SetParent(null, true);
            if (ps.main.scalingMode != ParticleSystemScalingMode.Hierarchy)
                ps.transform.localScale = scale;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(ps.gameObject, lifetime);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {



        if (collision.GetComponent<Player>() != null)
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();


            stats.DoDamage(player);


            DestroySelf();
        }

        
    }

 
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x, playerCheck.position.y - playerCheckDistance));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x, playerCheck.position.y - groundCheckDistance));
    }
}
