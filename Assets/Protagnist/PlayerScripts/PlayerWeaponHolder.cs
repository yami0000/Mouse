using UnityEngine;

public class PlayerWeaponHolder : MonoBehaviour
{



    public static PlayerWeaponHolder Instance;

    public GameObject Weapon;



    public bool isHolding = false;





    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        else
            Instance = this;
    }

    private void Update()
    {
        Vector2 direction = GameManager.Instance.GetMouse(true);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, PlayerManager.Instance.player.facingDir == 1 ? angle:180-angle);
    }
    public void EquipWeapon(ItemData_Equipment weaponData)
    {

        SpriteRenderer weapon = Weapon.GetComponent<SpriteRenderer>();

        if (weapon != null  )
        {
            weapon.sprite = weaponData.icon;

        }

        isHolding = true;
        }

    public void UnEquipWeapon()
    {
        SpriteRenderer weapon = Weapon.GetComponent<SpriteRenderer>();

        weapon.sprite = null;

        isHolding = false;


    }

    /// <summary>The held weapon sprite's transform: muzzle and Fire Effects offsets are measured from it.</summary>
    public Transform WeaponTransform => Weapon != null ? Weapon.transform : transform;

    /// <summary>Spawns the weapon's Fire Effects on the held weapon sprite, so they follow the aim.</summary>
    public void PlayFireFX(ItemData_Equipment weaponData)
    {
        weaponData.PlayFireEffects(WeaponTransform);
    }



}
