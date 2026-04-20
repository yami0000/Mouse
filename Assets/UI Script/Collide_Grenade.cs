using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collide_Grenade : Collide_Ammo
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
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {


                GameObject _Grenade = Instantiate(explode, gameObject.transform.position, Quaternion.identity);

                Destroy(gameObject);

                Destroy(_Grenade, 0.85f);
            }
        }
    }
}
