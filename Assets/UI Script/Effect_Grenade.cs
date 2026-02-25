using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ammo Effect", menuName = "Data/Ammo/Grenade")]
public class Effect_Grenade :ItemEffect
{
    [SerializeField] private GameObject AmmoPrefab;
    
    [SerializeField] private float yVelocity;

    private ItemData_Equipment Data;

    private void start()
    {
        GetPlayer().Timer = SK.Instance.Skill.EquippedSkill == "Reaper" && GetPlayer().UsingSkill ? Data.firingRate / 1.4f : Data.firingRate;
    }

    public override void ExecuteWeaponEffect(Transform _position, ItemData_Equipment data)
    {
        Data = data;
        if (GetPlayer().Timer > 0)
            return;



        Player player = PlayerManager.Instance.player;
        GameObject Ammo = Instantiate(AmmoPrefab, player.transform.position, player.transform.rotation);
        Ammo.GetComponent<Rigidbody2D>().velocity = new Vector2(data.xVelocity * player.facingDir, yVelocity);




        Destroy(Ammo, data.effectiveTime);

        GetPlayer().Timer = SK.Instance.Skill.EquippedSkill == "Reaper" && GetPlayer().UsingSkill ? Data.firingRate / 1.4f : Data.firingRate;
    }

    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }
}
