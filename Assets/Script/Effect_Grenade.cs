using UnityEngine;

// Lobs a grenade the way the player faces. The shot FX and fire rate live on the weapon.
public class Effect_Grenade :ItemEffect
{
    [SerializeField] private GameObject AmmoPrefab;

    [SerializeField] private float yVelocity;

    public override void ExecuteWeaponEffect(Transform _position, ItemData_Equipment data)
    {
        Player player = PlayerManager.Instance.player;
        Vector3 muzzle = data.MuzzlePosition(PlayerWeaponHolder.Instance.WeaponTransform);
        GameObject Ammo = Instantiate(AmmoPrefab, muzzle, player.transform.rotation);
        Ammo.GetComponent<Rigidbody2D>().velocity = new Vector2(data.xVelocity * player.facingDir, yVelocity);

        Destroy(Ammo, data.effectiveTime);
    }
}
