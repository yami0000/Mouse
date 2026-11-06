using System;
using UnityEngine;

/// <summary>
/// Bullets give off radiation: a circle of Radius around each one, lit in Light Color. An enemy in it
/// takes Base Damage + the weapon's Damage x Damage Rate, and again every Interval seconds while it
/// stays inside, for as long as the bullet exists. See RadiationField.
/// </summary>
[Serializable]
public class Mod_RadioactiveSubstance : ModEffect
{
    [Header("Radiation")]
    [Tooltip("Reach of the radiation from the bullet's centre, in world units.")]
    [Min(0.05f)]
    [SerializeField] private float radius = 1.5f;

    [Tooltip("Seconds between two hurts to the same enemy while it stays inside.")]
    [Min(0.05f)]
    [SerializeField] private float interval = 0.5f;

    [Tooltip("Damage per hurt before the weapon's share is added.")]
    [Min(0)]
    [SerializeField] private int baseDamage = 3;

    [Tooltip("Share of the weapon's Damage added to each hurt. 0.1 = 10%.")]
    [Min(0)]
    [SerializeField] private float damageRate = 0.1f;

    [Tooltip("What counts as an enemy. Nothing = the Enemy layer.")]
    [SerializeField] private LayerMask enemyLayers;

    [Header("Look")]
    [Tooltip("Light the circle with a 2D point light whose edge is the radius.")]
    [SerializeField] private bool showLight = true;
    [SerializeField] private Color lightColor = new Color(0.45f, 1f, 0.25f, 1f);
    [Min(0)]
    [SerializeField] private float lightIntensity = 1f;
    [Tooltip("Inside this distance the light is at full strength, then it fades out to the radius.")]
    [Min(0)]
    [SerializeField] private float lightInnerRadius = 0.2f;
    [Tooltip("How the light fades towards the edge (the light's Falloff Strength).")]
    [Range(0f, 1f)]
    [SerializeField] private float lightFalloff = 0.5f;

    [Tooltip("Optional extra visual (particles, a ring sprite...), made on the bullet's centre and carried with it.")]
    [SerializeField] private GameObject visualPrefab;

    public override void OnAmmoFired(ItemData_Equipment _equipment, AmmoBehaviour _ammo)
    {
        int damage = baseDamage + Mathf.RoundToInt(WeaponDamage() * damageRate);

        RadiationField field = _ammo.gameObject.AddComponent<RadiationField>();
        field.Setup(OwnerStats, radius, interval, damage, OrEnemyLayer(enemyLayers));

        if (showLight)
            field.AddLight(lightColor, lightIntensity, lightInnerRadius, lightFalloff);

        if (visualPrefab != null)
        {
            // Parented after placing it, keeping its world size: a small bullet would shrink it otherwise.
            GameObject visual = UnityEngine.Object.Instantiate(visualPrefab, _ammo.transform.position, Quaternion.identity);
            visual.transform.SetParent(_ammo.transform, true);
        }
    }
}
