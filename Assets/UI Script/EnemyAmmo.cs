using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Ammo Effect", menuName = "Data/Enemy Item/Ammo")]
public class EnemyAmmo : ItemEffect
{
    [SerializeField] public GameObject AmmoPrefab;

    [SerializeField] private Enemy_Bee enemy;
    
    
    [SerializeField] float Range;
    [SerializeField] float speed;

 
    public override void ExecuteEffect(Transform _position)
    {
        Player player = PlayerManager.Instance.player;

        Vector2 direction = (player.rb.position - enemy.rb.position ).normalized;

       
        GameObject Ammo = Instantiate(AmmoPrefab, enemy.rb.position, enemy.transform.rotation);
        Ammo.GetComponent<Rigidbody2D>().velocity = direction * speed;


       
        
 

        Destroy(Ammo, Range);

        
    }
 
}
