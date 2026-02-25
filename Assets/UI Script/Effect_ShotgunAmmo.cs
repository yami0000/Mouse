using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Ammo Effect", menuName = "Data/Ammo/Shotgun Ammo")]
public class Effect_ShotgunAmmo : ItemEffect
{
    [SerializeField] private GameObject AmmoPrefab;

    private ItemData_Equipment Data;

    [SerializeField] private int pelletCount;
    [SerializeField] private float spreadAngle;

    private void Start()
    {
        GetPlayer().Timer = SK.Instance.Skill.EquippedSkill == "Reaper" && GetPlayer().UsingSkill ? Data.firingRate / 1.4f : Data.firingRate;
    }

    public override void ExecuteWeaponEffect(Transform _position,ItemData_Equipment data)
    {
        Data = data;
        if (GetPlayer().Timer > 0)
            return;

        Player player = GetPlayer();

        Vector2 D = GameManager.Instance.GetMouse();

        float anglex = Mathf.Atan2(D.y, D.x) * Mathf.Rad2Deg;


        for (int i = 0; i < pelletCount; i++)
        {
            float angle = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);

            
            Vector2 direction = Quaternion.Euler(0, 0, anglex+angle) * Vector2.right ;

            
            GameObject Ammo = Instantiate(AmmoPrefab, player.transform.position, Quaternion.identity);
            Ammo.GetComponent<Rigidbody2D>().velocity = direction.normalized * data.xVelocity;

            Destroy(Ammo, data.effectiveTime);

        }

        GetPlayer().Timer = SK.Instance.Skill.EquippedSkill == "Reaper" && GetPlayer().UsingSkill ? Data.firingRate / 1.4f : Data.firingRate;
    }
    private static Player GetPlayer()
    {
        return PlayerManager.Instance.player;
    }
}
