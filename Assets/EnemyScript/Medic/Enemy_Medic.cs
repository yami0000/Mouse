using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemy_Medic : Enemy
{
    public MedicWalkingState walkState { get; private set; }
    public MedicIdleState idleState { get; private set; }
    public MedicBattleState battleState { get; private set; }
    public MedicHealState healState { get; private set; }
    public MedicDeathState deathState { get; private set; }

    public float stateTimer_Medic;

    [SerializeField]private LayerMask whatIsEnemy;

    [SerializeField] private float enemyCheckDistance;

    public float followingDistance;

    public Enemy _enemy;

    [SerializeField] public HealAmmo healAmmo;

    [SerializeField] public Test ParticleSystem;
    protected override void Awake()
    {
        base.Awake();

        walkState = new MedicWalkingState(this, stateMachine, "Walk", this);
        idleState = new MedicIdleState(this, stateMachine, "Idle", this);
        battleState = new MedicBattleState(this, stateMachine, "Walk", this);
        healState = new MedicHealState(this, stateMachine, "Heal", this);
        deathState = new MedicDeathState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(walkState);
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_Medic -= Time.deltaTime;
         

    }

    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deathState);
    }

    public void pinpointClosestEnemy()
    {

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, enemyCheckDistance, whatIsEnemy);

         

        float minDistance = Mathf.Infinity;
        Vector2 pos = transform.position;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject)
                continue;

            Enemy enemy = hit.gameObject.GetComponent<Enemy>();
            if (enemy == null)
                continue;
        
            float distance = Vector2.Distance(pos, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                _enemy = enemy;
            }


        }
    }


    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyCheckDistance);
    }
}

