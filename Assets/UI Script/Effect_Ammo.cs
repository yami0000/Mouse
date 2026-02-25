using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Ammo Effect", menuName = "Data/Ammo/Common Ammo")]
public class Effect_Ammo : ItemEffect 
{
    [SerializeField]private GameObject AmmoPrefab;
   
    private ItemData_Equipment Data; 

    private void start()
    {
        GetPlayer().Timer = SK.Instance.Skill.EquippedSkill == "Reaper" && GetPlayer().UsingSkill ? Data.firingRate / 1.4f : Data.firingRate;//如果技能发动，子弹发射时间间隙/1.4

    }


    public override void ExecuteWeaponEffect(Transform _position,ItemData_Equipment data)
    {
        Data = data;
        if(GetPlayer().Timer > 0)
        return;

         

        Player  player = GetPlayer();
        GameObject Ammo = Instantiate(AmmoPrefab, player.transform.position, player.transform.rotation);
        Ammo.GetComponent<Rigidbody2D>().velocity = GameManager.Instance.GetMouse() * data.xVelocity;






        Destroy(Ammo, data.effectiveTime);

        GetPlayer().Timer = SK.Instance.Skill.EquippedSkill == "Reaper" && GetPlayer().UsingSkill ? Data.firingRate / 1.4f : Data.firingRate;
    }

    

    

    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }


}
