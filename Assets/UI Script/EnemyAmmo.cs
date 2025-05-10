using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public class EnemyAmmo :MonoBehaviour
{
    [SerializeField] public GameObject AmmoPrefab;

    [SerializeField] private Enemy_Bee enemy;
    
    
    [SerializeField] float Range;
    [SerializeField] float speed;

     
    public  void ExecuteEffect(Transform _position)
    {
        Player player = PlayerManager.Instance.player;

        Vector2 direction = (player.rb.position - enemy.rb.position ).normalized;

       
        GameObject Ammo = Instantiate(AmmoPrefab, enemy.rb.position, enemy.transform.rotation);
        Ammo.GetComponent<Rigidbody2D>().velocity = direction * speed;


        EnemyAmmo_Effect _Ammo = Ammo.GetComponent<EnemyAmmo_Effect>();
        if (_Ammo != null)
            _Ammo.Initialize(enemy);
        
 

        Destroy(Ammo, Range);

        
    }

 
 
}
