using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ammo Effect", menuName = "Data/Ammo/Grenade")]
public class Effect_Grenade :ItemEffect
{
    [SerializeField] private GameObject AmmoPrefab;
    
    [SerializeField] private float yVelocity;
    [SerializeField] private GameObject ShootEffect;

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

        Effect(data, player);

        Destroy(Ammo, data.effectiveTime);

        GetPlayer().Timer = SK.Instance.Skill.EquippedSkill == "Reaper" && GetPlayer().UsingSkill ? Data.firingRate / 1.4f : Data.firingRate;
    }

    private void Effect(ItemData_Equipment data, Player player)
    {
        Vector2 lastVelocity = GameManager.Instance.GetMouse(true) * data.xVelocity;
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
