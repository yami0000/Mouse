using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A laser sight on the held weapon: a line from the muzzle (the weapon's Ammo Offset) along the
/// direction bullets fly, stopping at the first enemy or ground collider, or at Max Distance.
/// Shown while the weapon is held; gone when it's put away, unequipped or the mod comes out.
/// </summary>
[Serializable]
public class Mod_LaserPointer : ModEffect
{
    [Tooltip("Where the laser stops. Nothing = the Enemy and Ground layers.")]
    [SerializeField] private LayerMask stopAt;
    [Tooltip("Also stop at trigger colliders on those layers (e.g. an enemy's detection range). Usually off.")]
    [SerializeField] private bool stopAtTriggers;
    [Tooltip("Length when nothing is in the way, in world units.")]
    [Min(0.1f)]
    [SerializeField] private float maxDistance = 25f;

    [Header("Look")]
    [Min(0)]
    [SerializeField] private float width = 0.04f;
    [Tooltip("Color at the muzzle.")]
    [SerializeField] private Color startColor = new Color(1f, 0.15f, 0.15f, 0.9f);
    [Tooltip("Color at the far end.")]
    [SerializeField] private Color endColor = new Color(1f, 0.15f, 0.15f, 0.35f);
    [Tooltip("Line material. Empty = Sprites/Default, unlit.")]
    [SerializeField] private Material material;
    [Tooltip("Sorting layer name. Empty = Default.")]
    [SerializeField] private string sortingLayer = "";
    [SerializeField] private int sortingOrder = 50;
    [Tooltip("Optional dot shown where the laser ends, e.g. a small glowing sprite.")]
    [SerializeField] private GameObject endPrefab;

    private class Sight
    {
        public LineRenderer line;
        public Transform end;
    }

    private static readonly List<RaycastHit2D> hits = new List<RaycastHit2D>();
    private static Material defaultMaterial;

    public override void OnWornUpdate(ItemData_Equipment _equipment)
    {
        Sight sight = StateFor<Sight>(_equipment);

        PlayerWeaponHolder holder = PlayerWeaponHolder.Instance;
        ItemData_Equipment weapon = WornWeapon;

        if (holder == null || !holder.isHolding || weapon == null || GameManager.Instance == null)
        {
            Show(sight, false);
            return;
        }

        if (sight.line == null)
            Create(sight);

        Vector2 origin = weapon.MuzzlePosition(holder.WeaponTransform);
        Vector2 direction = GameManager.Instance.GetMouse(true).normalized;
        Vector2 end = origin + direction * maxDistance;

        LayerMask mask = stopAt.value != 0 ? stopAt : (LayerMask)LayerMask.GetMask("Enemy", "Ground");
        ContactFilter2D filter = new ContactFilter2D { useTriggers = stopAtTriggers };
        filter.SetLayerMask(mask);

        hits.Clear();
        if (Physics2D.Raycast(origin, direction, filter, hits, maxDistance) > 0)
        {
            // Results aren't promised in order, so take the nearest.
            float nearest = float.MaxValue;
            foreach (RaycastHit2D hit in hits)
                if (hit.distance < nearest)
                {
                    nearest = hit.distance;
                    end = hit.point;
                }
        }

        sight.line.SetPosition(0, origin);
        sight.line.SetPosition(1, end);
        if (sight.end != null)
            sight.end.position = end;

        Show(sight, true);
    }

    public override void OnWornStopped(ItemData_Equipment _equipment)
    {
        Sight sight = StateFor<Sight>(_equipment);

        if (sight.line != null)
            UnityEngine.Object.Destroy(sight.line.gameObject);

        sight.line = null;
        sight.end = null;
    }

    private void Create(Sight _sight)
    {
        GameObject laser = new GameObject("Laser Pointer");
        laser.transform.SetParent(ModEffects.Root, false);

        LineRenderer line = laser.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.widthMultiplier = width;
        line.startColor = startColor;
        line.endColor = endColor;
        line.numCapVertices = 2;
        line.sharedMaterial = material != null ? material : DefaultMaterial();
        line.sortingOrder = sortingOrder;
        if (!string.IsNullOrEmpty(sortingLayer))
            line.sortingLayerName = sortingLayer;

        _sight.line = line;

        if (endPrefab != null)
            _sight.end = UnityEngine.Object.Instantiate(endPrefab, laser.transform).transform;
    }

    private static void Show(Sight _sight, bool _show)
    {
        if (_sight.line != null && _sight.line.gameObject.activeSelf != _show)
            _sight.line.gameObject.SetActive(_show);
    }

    private static Material DefaultMaterial()
    {
        if (defaultMaterial == null)
            defaultMaterial = new Material(Shader.Find("Sprites/Default"));

        return defaultMaterial;
    }
}
