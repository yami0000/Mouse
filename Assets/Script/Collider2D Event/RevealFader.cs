using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// Added at runtime by RevealAction to each object it shows. Fades every SpriteRenderer,
/// SpriteShapeRenderer and UI Graphic (Image, Text, TextMeshPro) under the object between 0%
/// and its own opacity as set in the scene, so a half see-through sprite stays half see-through.
///
/// An object that is inactive in the scene is switched on to fade in and back off once faded out,
/// so its colliders only exist while it is shown.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("")]
public class RevealFader : MonoBehaviour
{
    // Objects revealed for good this play session. Static, so it survives scene loads.
    private static readonly HashSet<string> revealedForever = new HashSet<string>();

    private readonly List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    private readonly List<SpriteShapeRenderer> shapes = new List<SpriteShapeRenderer>();
    private readonly List<Graphic> graphics = new List<Graphic>();
    private readonly List<float> fullAlpha = new List<float>();

    private bool startedInactive;
    private float currentAlpha = 1f;
    private Coroutine routine;

    private Transform followTarget;
    private Vector3 followOffset;
    private Vector3 homePosition;

    public bool IsFading => routine != null;
    public bool IsVisible => currentAlpha > 0f || IsFading;
    public bool IsRevealedForever => revealedForever.Contains(Key);

    public static RevealFader For(GameObject _target)
    {
        if (_target.TryGetComponent(out RevealFader fader))
            return fader;

        fader = _target.AddComponent<RevealFader>();
        fader.Collect();
        return fader;
    }

    // Done in For rather than Awake: Awake doesn't run on an inactive object.
    private void Collect()
    {
        startedInactive = !gameObject.activeSelf;

        GetComponentsInChildren(true, sprites);
        GetComponentsInChildren(true, shapes);
        GetComponentsInChildren(true, graphics);

        foreach (var sr in sprites) fullAlpha.Add(sr.color.a);
        foreach (var ssr in shapes) fullAlpha.Add(ssr.color.a);
        foreach (var g in graphics) fullAlpha.Add(g.color.a);
    }

    /// <summary>Scene name + hierarchy path, so the same object is recognised after a scene reload.</summary>
    private string Key
    {
        get
        {
            string path = name;
            for (Transform t = transform.parent; t != null; t = t.parent)
                path = t.name + "/" + path;
            return gameObject.scene.name + ":" + path;
        }
    }

    public void MarkRevealedForever() => revealedForever.Add(Key);

    /// <summary>Hidden at once, or fully shown if it was revealed for good earlier this session.</summary>
    public void SetStartState()
    {
        if (IsRevealedForever)
            Show();
        else
            Hide();
    }

    public void Show()
    {
        Stop();
        gameObject.SetActive(true);
        SetAlpha(1f);
    }

    public void Hide()
    {
        Stop();
        SetAlpha(0f);
        StopFollowing();

        if (startedInactive)
            gameObject.SetActive(false);
    }

    /// <summary>Keeps this at _target's position + _offset until hidden, then puts it back where it was.</summary>
    public void Follow(Transform _target, Vector2 _offset)
    {
        if (followTarget == null)
            homePosition = transform.position;

        followTarget = _target;
        followOffset = _offset;
        StickToTarget();
    }

    private void StopFollowing()
    {
        if (followTarget == null) return;

        followTarget = null;
        transform.position = homePosition;
    }

    // Placed rather than parented, so it doesn't turn round with the player and read backwards.
    private void LateUpdate() => StickToTarget();

    private void StickToTarget()
    {
        if (followTarget == null) return;

        Vector3 position = followTarget.position + followOffset;
        position.z = transform.position.z;
        transform.position = position;
    }

    /// <summary>Fades to _alpha (0 to 1) over _duration, scaled by how far it has to go.</summary>
    public void FadeTo(float _alpha, float _duration)
    {
        Stop();

        if (_alpha > 0f)
            gameObject.SetActive(true);

        // Scaled by the distance left, so turning round mid-fade doesn't take the full time.
        float duration = _duration * Mathf.Abs(_alpha - currentAlpha);

        if (duration <= 0f || !isActiveAndEnabled)
        {
            if (_alpha > 0f) SetAlpha(_alpha);
            else Hide();
            return;
        }

        routine = StartCoroutine(Fade(_alpha, duration));
    }

    private IEnumerator Fade(float _alpha, float _duration)
    {
        float start = currentAlpha;
        float time = 0f;

        while (time < _duration)
        {
            time += Time.deltaTime;
            SetAlpha(Mathf.Lerp(start, _alpha, time / _duration));
            yield return null;
        }

        routine = null;

        if (_alpha > 0f) SetAlpha(_alpha);
        else Hide();
    }

    private void Stop()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = null;
    }

    private void SetAlpha(float _alpha)
    {
        currentAlpha = _alpha;
        int i = 0;

        foreach (var sr in sprites)
        {
            float full = fullAlpha[i++];
            if (sr == null) continue;
            Color c = sr.color; c.a = full * _alpha; sr.color = c;
        }

        foreach (var ssr in shapes)
        {
            float full = fullAlpha[i++];
            if (ssr == null) continue;
            Color c = ssr.color; c.a = full * _alpha; ssr.color = c;
        }

        foreach (var g in graphics)
        {
            float full = fullAlpha[i++];
            if (g == null) continue;
            Color c = g.color; c.a = full * _alpha; g.color = c;
        }
    }

    private void OnDisable()
    {
        // Coroutines die with the object, clear the handle so IsFading doesn't stay true forever.
        routine = null;
    }
}
