using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Artiliary : Enemy
{
    public ArtiliaryWalkState walkState { get; private set; }
    public ArtiliaryIdleState idleState { get; private set; }
    public ArtiliaryBattleState battleState { get; private set; }
    public ArtiliaryAttackState attackState { get; private set; }
    public ArtiliaryDeathState deathState { get; private set; }

    public float stateTimer_Artiliary;

    // [SerializeField] private LayerMask whatIsEnemy;

    //[SerializeField] private float enemyCheckDistance;

    //public float followingDistance;

    // public Enemy _enemy;

    //[SerializeField] public HealAmmo healAmmo;
    //[SerializeField] public EnemyAmmo ammo;

    // [SerializeField] public Test ParticleSystem;
     public Projectile projectile;
    protected override void Awake()
    {
        base.Awake();

        walkState = new ArtiliaryWalkState(this, stateMachine, "Walk", this);
        idleState = new ArtiliaryIdleState(this, stateMachine, "Idle", this);
        battleState = new ArtiliaryBattleState(this, stateMachine, "Walk", this);
        attackState = new ArtiliaryAttackState(this, stateMachine, "Attack", this);
        deathState = new ArtiliaryDeathState(this, stateMachine, "Idle", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(walkState);
    }

    protected override void Update()
    {
        base.Update();
        stateTimer_Artiliary -= Time.deltaTime;


    }

    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deathState);
    }


    public bool CanAttack()
    {
        if (Time.time >= lastTimeAttacked + attackCooldown)
        {
             
            return true;
        }

        return false;

    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerCheckDistance);
    }
}
