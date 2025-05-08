using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserAttack : MonoBehaviour
{
    [SerializeField] private Laser laser;
    [SerializeField] private float prepTime;
    [SerializeField] private float chargingTime;
    [SerializeField] private float fireLaserTime;
    [SerializeField] private float cooldown;
    [SerializeField] LayerMask whatIsPlayer;            

    [SerializeField] Detection detection;

    public bool isAttack = false;

    private bool hitBefore = false;

    [SerializeField] Enemy_Laser enemy;

    [SerializeField]private EnemyStats EnemyStats;


    public void LaunchLaser()
    {
        if (   !isAttack)
            StartCoroutine(AttackSequence());

    }

    IEnumerator AttackSequence() 
    {
        isAttack = true;
        float t = 0f;

       //Debug.Log("preparing");
        while (t < prepTime)
        {
        t += Time.deltaTime;
        yield return null;
        }

        t = 0f;

      // Debug.Log("charging");
        laser.ShowChargeLaser();
        while (t < chargingTime)
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
            { PlayerStats player = PlayerManager.Instance.player.GetComponent<PlayerStats>();
                hitBefore = true;
                EnemyStats.DoDamage(player);
                
            }
               
           

            t += Time.deltaTime;
            
            yield return null;
        }
        laser.HideLaser();
        t = 0f;
        hitBefore = false;

      // Debug.Log("cooldown");
        while (t < cooldown)
        {
            t += Time.deltaTime;
            yield return null;
        }

        isAttack = false;
         
        
        t = 0f;

        
    }
}
