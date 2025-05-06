using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Ammo Effect", menuName = "Data/Ammo/Common Ammo")]
public class Effect_Ammo : ItemEffect 
{
    [SerializeField]private GameObject AmmoPrefab;
    [SerializeField] private float xVelocity;
    [SerializeField] float Range;
    [SerializeField] float speed;
 

    private void start()
    {
    PlayerManager.Instance.player.Timer = speed;
       
     }
 
    public override void ExecuteEffect(Transform _position)
    {
        if(PlayerManager.Instance.player.Timer > 0)
        return;


        
        Player  player = PlayerManager.Instance.player;
        GameObject Ammo = Instantiate(AmmoPrefab, player.transform.position, player.transform.rotation);
         Ammo.GetComponent<Rigidbody2D>().velocity = new Vector2(xVelocity * player.facingDir,0);

    


            Destroy(Ammo, Range);

        PlayerManager.Instance.player.Timer = speed;
    }


}
