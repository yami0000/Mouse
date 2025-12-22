using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class DroneA : MonoBehaviour
{
    [Header("Follow Settings")]
    private Transform player;
    public float followSpeed = 3f;
    public float followRadius = 2f; // Distance before drone starts moving
    public float yOffset = 1.5f;    // Minimum height above player

    [Header("Floating Settings")]
    public float floatAmplitude = 0.5f; // How high/low it floats
    public float floatSpeed = 1f;       // How fast it floats

    [Header("Check Enemies")]
     [SerializeField]private LayerMask whatIsEnemy;

    [SerializeField] private float enemyCheckDistance;
    [Header("Ammo Stats")]
    [SerializeField] private GameObject AmmoPrefab;
    [SerializeField] private float velocity;
    [SerializeField] private float Range;
    [SerializeField] private float CD;
    private float CoolDown = 0f;

    

    private  Enemy _enemy;

    private float _startRelativeY;

    void Start()
    {
        player = PlayerManager.Instance.player.transform;
    }
    void Update()
    {
        CoolDown -= Time.deltaTime;
        Track();
        if (CoolDown <= 0)
        {
            CoolDown = CD;
            pinpointClosestEnemy();
            if (_enemy != null)
                shoot();
            else
                return;

        }
    }

    private void Track()
    {
        if (player == null) return;


        float targetY = player.position.y + yOffset;


        float hoverOffset = Mathf.Lerp(-floatAmplitude, floatAmplitude, Mathf.PingPong(Time.time * floatSpeed, 1.0f));
        targetY += hoverOffset;


        Vector3 targetPos = new Vector3(player.position.x, targetY, transform.position.z);


        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > followRadius)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
        else
        {

            float verticalLerp = Mathf.Lerp(transform.position.y, targetY, followSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, verticalLerp, transform.position.z);
        }
    }

    Vector3 CalculateTargetPosition()
    {
        Vector3 target = player.position;

         
        target.y = Mathf.Max(transform.position.y, player.position.y + yOffset);

        
        return target;
    }

    float GetHoverOffset()
    {
       
        float t = Mathf.PingPong(Time.time * floatSpeed, 1.0f);

        
        return Mathf.Lerp(-floatAmplitude, floatAmplitude, t);
    }

    public void pinpointClosestEnemy()
    {

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, enemyCheckDistance, whatIsEnemy);

        if (hits.Length == 0)
        {
            _enemy = null;
            return;
        }


        float minDistance = Mathf.Infinity;
        Vector2 pos = transform.position;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject)
                continue;




            Enemy enemy = hit.gameObject.GetComponent<Enemy>();


            float distance = Vector2.Distance(pos, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                _enemy = enemy;
            }

        }
    }

    public void shoot()
    {

        Vector2 targetPosition = _enemy.transform.position;
        Vector2 currentPosition = transform.position;
        Vector2 shootDirection = (targetPosition - currentPosition).normalized;


        GameObject Ammo = Instantiate(AmmoPrefab, transform.position, Quaternion.identity);



        float Velocity = velocity ;

        Rigidbody2D rb = Ammo.GetComponent<Rigidbody2D>();
        if (rb != null)
        {

            rb.velocity = shootDirection * Mathf.Abs(Velocity);
        }

        
          
        Destroy(Ammo, Range);


    }
}
     
    
