using UnityEngine;

// Fires a spread of pellets around the aim. The shot FX live on the weapon (Fire Effects).
public class Effect_ShotgunAmmo : ItemEffect
{
    [SerializeField] private GameObject AmmoPrefab;

    [SerializeField] private int pelletCount;
    [SerializeField] private float spreadAngle;

    public override void ExecuteWeaponEffect(Transform _position,ItemData_Equipment data)
    {
        Vector3 muzzle = data.MuzzlePosition(PlayerWeaponHolder.Instance.WeaponTransform);

        Vector2 D = GameManager.Instance.GetMouse(true);

        float anglex = Mathf.Atan2(D.y, D.x) * Mathf.Rad2Deg;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);


            Vector2 direction = Quaternion.Euler(0, 0, anglex + angle) * Vector2.right;


            GameObject Ammo = Instantiate(AmmoPrefab, muzzle, Quaternion.identity);
            Ammo.GetComponent<Rigidbody2D>().velocity = direction.normalized * data.xVelocity;

            Destroy(Ammo, data.effectiveTime);

        }
    }
}
