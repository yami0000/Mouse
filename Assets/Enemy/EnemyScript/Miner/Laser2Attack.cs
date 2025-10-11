using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Laser2Attack : MonoBehaviour
{
    [SerializeField] private Laser laser;
    [SerializeField] private float prepTime1;
    [SerializeField] private float chargingTime;
    [SerializeField] private float fireLaserTime;
    [SerializeField] private float prepTime2;
    [SerializeField] private float cooldown;
    [SerializeField] LayerMask whatIsPlayer;

    //[SerializeField] Detection detection;

    public bool isAttack = false;

    private bool hitBefore = false;

    [SerializeField] Enemy_Miner enemy;

    [SerializeField] private EnemyStats EnemyStats;

    public void LaunchLaser()
    {
        if (!isAttack)
            StartCoroutine(AttackSequence());

    }

    IEnumerator AttackSequence()
    {
        isAttack = true;
        float t = 0f;

        while (t < prepTime1)
        {
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;

        laser.ShowChargeLaser();

        while (t < chargingTime )
        {

            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;

        // Debug.Log("firing");
        laser.FireLaser();

        while (t < fireLaserTime)
        {
            RaycastHit2D hitplayer = Physics2D.Raycast(transform.position, laser.laserDirection, laser.laserLenth, whatIsPlayer);

            if (hitplayer && !hitBefore)
            {
                PlayerStats player = PlayerManager.Instance.player.GetComponent<PlayerStats>();
                hitBefore = true;
                EnemyStats.DoDamage(player);

            }



            t += Time.deltaTime;

            yield return null;
        }
        laser.HideLaser();
        t = 0f;


        while (t < prepTime2)
        {
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;

        laser.ShowChargeLaser();

        while (t < chargingTime )
        {

            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;

        // Debug.Log("firing");
        laser.FireLaser();

        while (t < fireLaserTime)
        {
            RaycastHit2D hitplayer = Physics2D.Raycast(transform.position, laser.laserDirection, laser.laserLenth, whatIsPlayer);

            if (hitplayer && !hitBefore)
            {
                PlayerStats player = PlayerManager.Instance.player.GetComponent<PlayerStats>();
                hitBefore = true;
                EnemyStats.DoDamage(player);

            }



            t += Time.deltaTime;

            yield return null;
        }
        laser.HideLaser();
        t = 0f;
        hitBefore = false;

        while (t < cooldown)
        {
            t += Time.deltaTime;
            yield return null;
        }

        isAttack = false;


        t = 0f;

    }
    }
