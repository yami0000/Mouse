using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows hidden objects (a tutorial sign, a secret platform...) by fading them from 0% to full
/// opacity, keeps them up for a while or for good, then fades them out again.
/// The objects are hidden from the start of the scene; the fading runs on a RevealFader on each.
/// </summary>
[System.Serializable]
public class RevealAction
{
    [Tooltip("Objects to show. Everything under each one fades: sprites, sprite shapes, UI and TextMeshPro text. " +
             "An object switched off in the scene is switched on to show, and off again once hidden.")]
    public List<GameObject> targets = new List<GameObject>();

    [Tooltip("Seconds to fade in from 0% to full opacity.")]
    [Min(0)] public float fadeInTime = 0.5f;

    [Tooltip("Shows the objects over the player's head and keeps them there while shown. " +
             "Once hidden they go back to where they were in the scene.")]
    public bool followPlayer;

    [Tooltip("Where the objects sit, from the player's position. Y 2.5 is where the E prompt sits.")]
    public Vector2 headOffset = new Vector2(0f, 2.5f);

    [Tooltip("Once shown, stays shown for good, even after leaving the scene and coming back.")]
    public bool permanent;

    [Tooltip("Stays shown until the player walks out of the collider (if that is ticked), with no time limit.")]
    public bool unlimited;

    [Tooltip("Seconds it stays fully shown before fading out.")]
    [Min(0)] public float showTime = 3f;

    [Tooltip("Fades out as soon as the player walks out of this event's collider.")]
    public bool hideWhenPlayerLeaves = true;

    [Tooltip("Seconds to fade out.")]
    [Min(0)] public float fadeOutTime = 0.5f;

    [Tooltip("Shows again each time the player walks back into the collider, once this step has run.")]
    public bool showAgainOnReenter = true;

    [Tooltip("The next step waits until this has faded out again. Off = the next step can start once it has faded in.")]
    public bool waitUntilHidden;

    // Bumped by every Run, so an older run that is still counting down knows a newer one took over.
    [System.NonSerialized] private int runId;

    public bool HasRun => runId > 0;

    /// <summary>Hides the targets at scene start, or shows them if they were revealed for good earlier.</summary>
    public void SetStartState()
    {
        foreach (RevealFader fader in Faders())
        {
            fader.SetStartState();

            if (followPlayer && fader.IsRevealedForever)
                FollowPlayer(fader);
        }
    }

    /// <summary>
    /// Fades in, then waits for the hide unless that runs on its own (see Wait Until Hidden).
    /// _host runs the hide when it does; _playerLeft says whether the player has walked out since this started.
    /// Running again while still shown restarts the show time.
    /// </summary>
    public IEnumerator Run(MonoBehaviour _host, System.Func<bool> _playerLeft)
    {
        int run = ++runId;
        List<RevealFader> faders = Faders();

        foreach (RevealFader fader in faders)
        {
            if (followPlayer)
                FollowPlayer(fader);

            fader.FadeTo(1f, fadeInTime);
        }

        if (permanent)
        {
            foreach (RevealFader fader in faders)
                fader.MarkRevealedForever();

            yield return WaitWhileFading(faders);
            yield break;
        }

        IEnumerator hide = HideLater(faders, _playerLeft, run);

        if (waitUntilHidden)
        {
            yield return hide;
        }
        else
        {
            _host.StartCoroutine(hide);
            yield return new WaitForSeconds(fadeInTime);
        }
    }

    private IEnumerator HideLater(List<RevealFader> _faders, System.Func<bool> _playerLeft, int _run)
    {
        float time = 0f;

        // The show time counts from fully visible; leaving the collider counts at any point.
        // A newer run takes over the countdown.
        while (_run == runId && (unlimited || time < fadeInTime + showTime))
        {
            if (hideWhenPlayerLeaves && _playerLeft())
                break;

            time += Time.deltaTime;
            yield return null;
        }

        if (_run == runId)
            foreach (RevealFader fader in _faders)
                fader.FadeTo(0f, fadeOutTime);

        // Waits for hidden rather than for the fade, so a step waiting on this isn't let go early
        // when the player walks back in mid fade out.
        while (_faders.Exists(f => f != null && f.IsVisible))
            yield return null;
    }

    private void FollowPlayer(RevealFader _fader)
    {
        Player player = PlayerManager.Instance != null ? PlayerManager.Instance.player : null;
        if (player != null)
            _fader.Follow(player.transform, headOffset);
    }

    private static IEnumerator WaitWhileFading(List<RevealFader> _faders)
    {
        while (_faders.Exists(f => f != null && f.IsFading))
            yield return null;
    }

    private List<RevealFader> Faders()
    {
        var faders = new List<RevealFader>();

        foreach (GameObject target in targets)
            if (target != null)
                faders.Add(RevealFader.For(target));

        return faders;
    }
}
