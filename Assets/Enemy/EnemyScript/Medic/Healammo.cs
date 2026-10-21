using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAmmo : MonoBehaviour
{
    [SerializeField] public GameObject AmmoPrefab;

    [SerializeField] private Enemy_Medic enemy;

    [SerializeField] float DestroyTime;
    [SerializeField] float speed;

    public void ExecuteEffect(Transform _position)
    {

        Vector2 direction = (  enemy._enemy.transform.position -enemy.transform.position).normalized;

        GameObject healammo = Instantiate(AmmoPrefab, enemy.transform.position, enemy.transform.rotation);
        healammo.GetComponent<Rigidbody2D>().velocity = direction * speed;



        AmmoBehaviour _healammo = healammo.GetComponent<AmmoBehaviour>();  
        if (_healammo != null )
            _healammo.Initialize(enemy);

        Destroy(healammo,DestroyTime);


    }
}