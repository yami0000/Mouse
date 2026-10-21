using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Attach to a GameObject with a Collider2D set to "Is Trigger".
/// When the Player enters, the target renderers fade out to 0% opacity;
/// when the Player exits, they fade back to 100%.
/// Supports both SpriteRenderer and SpriteShapeRenderer.
/// </summary>
public class FadeZone : DETECTION
{
    [Tooltip("Sprites to fade. Leave empty to auto-collect from this object and its children.")]
    [SerializeField] private SpriteRenderer[] spriteTargets;

    [Tooltip("Sprite shapes to fade. Leave empty to auto-collect from this object and its children.")]
    [SerializeField] private SpriteShapeRenderer[] shapeTargets;

    [Tooltip("Seconds for a full 0% <-> 100% fade.")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Tooltip("Once entered, stay invisible forever - even after leaving and coming back from other scenes.")]
    [SerializeField] private bool disappearForever = false;

    [Tooltip("Optional unique ID for remembering this zone. Leave empty to use scene name + hierarchy path.")]
    [SerializeField] private string persistentId;

    // Zones that have been permanently revealed this play session.
    // Static, so it survives scene loads.
    private static readonly HashSet<string> goneForever = new HashSet<string>();

    // No floating E prompt for a passive zone
    protected override bool ShowPrompt => false;

    private Coroutine fadeRoutine;
    private float currentAlpha = 1f;
    private bool isGone;

    private void Awake()
    {
        if (spriteTargets == null || spriteTargets.Length == 0)
            spriteTargets = GetComponentsInChildren<SpriteRenderer>();

        if (shapeTargets == null || shapeTargets.Length == 0)
            shapeTargets = GetComponentsInChildren<SpriteShapeRenderer>();

        if (disappearForever && goneForever.Contains(GetKey()))
        {
            isGone = true;
            SetAlpha(0f);
        }
    }

    protected override void OnPlayerEnter()
    {
        if (disappearForever && !isGone)
        {
            isGone = true;
            goneForever.Add(GetKey());
        }
        StartFade(0f);
    }

    protected override void OnPlayerExit()
    {
        if (isGone) return;
        StartFade(1f);
    }

    private string GetKey()
    {
        if (!string.IsNullOrEmpty(persistentId)) return persistentId;

        string path = name;
        for (Transform t = transform.parent; t != null; t = t.parent)
            path = t.name + "/" + path;
        return gameObject.scene.name + ":" + path;
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(targetAlpha));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = currentAlpha;

        // Scale duration by remaining distance, so leaving mid-fade
        // doesn't take the full duration to recover.
        float duration = fadeDuration * Mathf.Abs(targetAlpha - startAlpha);

        if (duration <= 0f)
        {
            SetAlpha(targetAlpha);
            fadeRoutine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, t / duration));
            yield return null;
        }

        SetAlpha(targetAlpha);
        fadeRoutine = null;
    }

    private void SetAlpha(float a)
    {
        currentAlpha = a;

        foreach (SpriteRenderer sr in spriteTargets)
        {
            if (sr == null) continue;
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }

        foreach (SpriteShapeRenderer ssr in shapeTargets)
        {
            if (ssr == null) continue;
            Color c = ssr.color;
            c.a = a;
            ssr.color = c;
        }
    }

    private void OnDisable()
    {
        // Coroutines stop automatically when the object is disabled;
        // clear the stale handle so re-enabling starts clean.
        fadeRoutine = null;
    }
}
