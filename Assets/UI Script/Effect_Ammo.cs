using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
[CreateAssetMenu(fileName = "Ammo Effect", menuName = "Data/Ammo/Common Ammo")]
public class Effect_Ammo : ItemEffect 
{
    [SerializeField] private GameObject AmmoPrefab;
    [SerializeField] private GameObject ShootEffect;
   

    private ItemData_Equipment Data; 

    private void start()
    {
        GetPlayer().Timer = SK.Instance.Skill.EquippedSkill == "Reaper" && GetPlayer().UsingSkill ? Data.firingRate / 1.4f : Data.firingRate;//如果技能发动，子弹发射时间间隙/1.4

    }


    public override void ExecuteWeaponEffect(UnityEngine.Transform _position,ItemData_Equipment data)
    {
        Data = data;

        Player player = GetPlayer();

        GameObject Ammo = Instantiate(AmmoPrefab, player.transform.position, player.transform.rotation);
        Ammo.GetComponent<Rigidbody2D>().velocity = GameManager.Instance.GetMouse() * data.xVelocity;

        Effect(data, player);

        Destroy(Ammo, data.effectiveTime);



        GetPlayer().Timer = SK.Instance.Skill.EquippedSkill == "Reaper" && GetPlayer().UsingSkill ? Data.firingRate / 1.4f : Data.firingRate;
    }

    private void Effect(ItemData_Equipment data, Player player)
    {
        Vector2 lastVelocity = GameManager.Instance.GetMouse() * data.xVelocity;
        Vector3 Pos = new Vector3(player.transform.position.x,
                                  player.transform.position.y - 0.9f,
                                  player.transform.position.z);


        float angle = Mathf.Atan2(lastVelocity.y, lastVelocity.x) * Mathf.Rad2Deg;



        GameObject effect = Instantiate(ShootEffect, Pos, Quaternion.Euler(0, 0, angle - 90));

        ParticleSystem ps = effect.GetComponentInChildren<ParticleSystem>();

        Destroy(effect, ps.main.duration);
    }




    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }

   
}
