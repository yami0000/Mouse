using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Put on a bullet by Mod_RadioactiveSubstance. While the bullet exists, every enemy inside Radius
/// takes Damage, then again every Interval seconds for as long as it stays inside.
///
/// The circle is checked with Physics2D.OverlapCircle rather than a trigger CircleCollider2D on the
/// bullet: a trigger there would join the bullet's own Rigidbody2D, so the bullet itself would "hit"
/// everything the circle touched. The light, sized to the radius, shows where it reaches.
/// </summary>
public class RadiationField : MonoBehaviour
{
    private float radius;
    private float interval;
    private int damage;
    private EntityStats owner;
    private ContactFilter2D filter;

    // When each enemy may next be hurt. Kept after it leaves, so stepping out and back in doesn't hurt it sooner.
    private readonly Dictionary<EntityStats, float> nextHit = new Dictionary<EntityStats, float>();
    private static readonly List<Collider2D> hits = new List<Collider2D>();

    public float Radius => radius;

    public void Setup(EntityStats _owner, float _radius, float _interval, int _damage, LayerMask _enemyLayers)
    {
        owner = _owner;
        radius = _radius;
        interval = Mathf.Max(0.05f, _interval);
        damage = _damage;

        filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(_enemyLayers);
    }

    /// <summary>A Point light at the centre, its outer edge on the radius.</summary>
    public void AddLight(Color _color, float _intensity, float _innerRadius, float _falloff)
    {
        GameObject lightObject = new GameObject("Radiation Light");
        lightObject.transform.SetParent(transform, false);

        Light2D light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.pointLightOuterRadius = radius;
        light.pointLightInnerRadius = Mathf.Clamp(_innerRadius, 0f, radius);
        light.color = _color;
        light.intensity = _intensity;
        light.falloffIntensity = _falloff;
    }

    private void FixedUpdate()
    {
        if (owner == null || damage <= 0) return;

        hits.Clear();
        Physics2D.OverlapCircle(transform.position, radius, filter, hits);

        float now = Time.time;

        foreach (Collider2D hit in hits)
        {
            EntityStats enemy = hit.GetComponent<Enemy>() != null ? hit.GetComponent<EnemyStats>() : null;
            if (enemy == null) continue;

            // An enemy with several colliders is only hurt once per tick: the first sets its next time.
            if (nextHit.TryGetValue(enemy, out float next) && now < next) continue;

            nextHit[enemy] = now + interval;
            owner.DoFixedDamage(enemy, damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 1f, 0.3f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
