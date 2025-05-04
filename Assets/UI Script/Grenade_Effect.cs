using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade_Effect : Ammo_Effect
{
    [SerializeField]private GameObject explode;
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (enemy != null)
        {
            GameObject Grenade = Instantiate(explode, enemy.position, Quaternion.identity);

            Destroy(Grenade, 0.85f);

             
        }
        if (enemy != null)
            return;
        {
            if (IsGroundDetected()|| IsWallDetected())
            {


                GameObject _Grenade = Instantiate(explode, gameObject.transform.position, Quaternion.identity);

                Destroy(gameObject);

                Destroy(_Grenade, 0.85f);
            }
        }
    }
}
