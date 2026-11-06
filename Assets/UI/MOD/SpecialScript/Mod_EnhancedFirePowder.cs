using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every shot also lets off a shockwave at the muzzle (the weapon's Ammo Offset), pointing along the shot.
/// It's instant: every enemy inside takes the weapon's Damage x Damage Rate once, on the frame it fires.
/// The area is a trapezoid, narrow at the muzzle and wide at the far end, unless Shockwave Prefab has
/// colliders of its own, which then are the area.
/// </summary>
[Serializable]
public class Mod_EnhancedFirePowder : ModEffect
{
    [Tooltip("Share of the weapon's Damage each enemy in the shockwave takes. 0.5 = 50%.")]
    [Min(0)]
    [SerializeField] private float damageRate = 0.5f;

    [Tooltip("What counts as an enemy. Nothing = the Enemy layer.")]
    [SerializeField] private LayerMask enemyLayers;

    [Header("Area (when the prefab has no colliders)")]
    [Tooltip("Width of the trapezoid at the muzzle.")]
    [Min(0)]
    [SerializeField] private float nearWidth = 0.6f;
    [Tooltip("Width at its far end.")]
    [Min(0)]
    [SerializeField] private float farWidth = 2f;
    [Tooltip("How far it reaches along the shot from the muzzle.")]
    [Min(0.01f)]
    [SerializeField] private float length = 2.5f;
    [Tooltip("Moves its start forward (+) or back (-) along the shot from the muzzle.")]
    [SerializeField] private float startOffset;

    [Header("Look")]
    [Tooltip("Optional: spawned at the muzzle facing along the shot (+X forward), flipped when aiming left. " +
             "Give it Collider2Ds (e.g. a Polygon Collider 2D) to set the area yourself; they're only used for the check.")]
    [SerializeField] private GameObject shockwavePrefab;
    [Tooltip("Seconds until the prefab is destroyed. 0 = once its animation / particles have played through.")]
    [Min(0)]
    [SerializeField] private float prefabLifetime;
    [Tooltip("Outline the area in the Scene view for a moment on each shot, for tuning.")]
    [SerializeField] private bool drawArea = true;

    private static readonly List<Collider2D> found = new List<Collider2D>();
    private static readonly HashSet<EntityStats> damaged = new HashSet<EntityStats>();

    public override void OnShot(ItemData_Equipment _weapon)
    {
        PlayerWeaponHolder holder = PlayerWeaponHolder.Instance;
        if (holder == null) return;

        Vector2 direction = GameManager.Instance.GetMouse(true).normalized;
        Vector2 origin = (Vector2)_weapon.MuzzlePosition(holder.WeaponTransform) + direction * startOffset;
        Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        bool aimingLeft = direction.x < 0f;

        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(OrEnemyLayer(enemyLayers));

        found.Clear();
        bool customArea = false;

        if (shockwavePrefab != null)
        {
            GameObject shockwave = UnityEngine.Object.Instantiate(shockwavePrefab, origin, rotation);
            if (aimingLeft)
            {
                Vector3 scale = shockwave.transform.localScale;
                shockwave.transform.localScale = new Vector3(scale.x, -scale.y, scale.z);
            }

            Collider2D[] areas = shockwave.GetComponentsInChildren<Collider2D>();
            if (areas.Length > 0)
            {
                customArea = true;
                Physics2D.SyncTransforms();

                foreach (Collider2D area in areas)
                {
                    Collect(area, filter);
                    area.enabled = false; // the check is done; it shouldn't push or trigger anything
                }
            }

            UnityEngine.Object.Destroy(shockwave, prefabLifetime > 0f ? prefabLifetime : StateFX.OneLoopLength(shockwave, 1f));
        }

        if (!customArea)
            CollectTrapezoid(origin, rotation, filter);

        int damage = Mathf.RoundToInt(WeaponDamage() * damageRate);
        EntityStats owner = OwnerStats;

        damaged.Clear();
        foreach (Collider2D hit in found)
        {
            EnemyStats enemy = EnemyOf(hit);
            if (enemy != null && damaged.Add(enemy))
                owner.DoFixedDamage(enemy, damage);
        }
    }

    private void CollectTrapezoid(Vector2 _origin, Quaternion _rotation, ContactFilter2D _filter)
    {
        Vector2[] points =
        {
            new Vector2(0f, -nearWidth / 2f),
            new Vector2(length, -farWidth / 2f),
            new Vector2(length, farWidth / 2f),
            new Vector2(0f, nearWidth / 2f),
        };

        // A throwaway trigger in the trapezoid's shape, used once for the overlap check.
        GameObject area = new GameObject("Shockwave Area");
        area.transform.SetPositionAndRotation(_origin, _rotation);

        PolygonCollider2D polygon = area.AddComponent<PolygonCollider2D>();
        polygon.isTrigger = true;
        polygon.points = points;

        Physics2D.SyncTransforms();
        Collect(polygon, _filter);

        polygon.enabled = false;
        UnityEngine.Object.Destroy(area);

        if (drawArea)
            for (int i = 0; i < points.Length; i++)
                Debug.DrawLine(_origin + (Vector2)(_rotation * points[i]),
                               _origin + (Vector2)(_rotation * points[(i + 1) % points.Length]),
                               new Color(1f, 0.6f, 0.2f), 0.3f);
    }

    private static void Collect(Collider2D _area, ContactFilter2D _filter)
    {
        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapCollider(_area, _filter, results);
        found.AddRange(results);
    }
}
