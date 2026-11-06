using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

/// <summary>
/// Shows the speaker's portrait next to the dialogue, looked up in the Character Database by the
/// name before the colon in a Yarn line ("Grandma: Hello!").
/// A character with a Portrait Animator is animated instead: its clips animate a SpriteRenderer,
/// so a hidden one runs the controller and LateUpdate copies its sprite to the Image.
/// "#anim:State" at the end of a line plays that state for the line.
/// </summary>
public class PortraitDialogueUI : DialoguePresenterBase
{
    [Header("Portrait Settings")]
    [SerializeField] private Image portraitImage;
    [Tooltip("Every speaker's portrait (Create > Dialogue > Character Database).")]
    [SerializeField] private CharacterDatabase database;
    [Tooltip("For lines with no speaker, or one missing from the database. Empty = hide the portrait.")]
    [SerializeField] private Sprite defaultPortrait;

    private Animator animator;
    private SpriteRenderer animatedSprite;
    // A line picked a state with #anim, so the next line without one goes back to the default state.
    private bool playedTaggedState;
    // Missing speakers are only warned about once.
    private readonly HashSet<string> warnedNames = new HashSet<string>();

    private void Awake()
    {
        CreateAnimator();
    }

    // Also called from RunLineAsync, in case this object started inactive and Awake hasn't run yet.
    private void CreateAnimator()
    {
        if (animator != null) return;

        GameObject animated = new GameObject("Portrait Animator");
        animated.transform.SetParent(transform, false);

        animatedSprite = animated.AddComponent<SpriteRenderer>();
        // Enabled so the clips can drive it, but never drawn: the Image shows its sprite.
        animatedSprite.forceRenderingOff = true;

        animator = animated.AddComponent<Animator>();
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // its renderer never counts as visible
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;    // keeps moving while the game is paused
    }

    private void LateUpdate()
    {
        // The animator has updated by now, so this frame's sprite is ready.
        if (animator.runtimeAnimatorController == null || animatedSprite.sprite == null) return;

        if (portraitImage.sprite != animatedSprite.sprite)
            SetSprite(animatedSprite.sprite);
    }

    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        CreateAnimator();

        CharacterProfile character = FindCharacter(line.CharacterName);

        ShowCharacter(character);
        PlayTaggedState(line.Metadata);

        return YarnTask.CompletedTask;
    }

    // Options are left to the options presenter.
    public override YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        return DialogueRunner.NoOptionSelected;
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        // Nothing to animate until the next conversation.
        if (animator != null)
            animator.runtimeAnimatorController = null;
        playedTaggedState = false;

        return YarnTask.CompletedTask;
    }

    private CharacterProfile FindCharacter(string characterName)
    {
        if (string.IsNullOrEmpty(characterName)) return null;

        CharacterProfile character = database != null ? database.Find(characterName) : null;

        if (character == null && warnedNames.Add(characterName))
            Debug.LogWarning($"[PortraitDialogueUI] '{characterName}' isn't in the Character Database" +
                             (database == null ? " (no database assigned)." : "."), this);

        return character;
    }

    private void ShowCharacter(CharacterProfile character)
    {
        RuntimeAnimatorController controller = character?.portraitAnimator;

        if (animator.runtimeAnimatorController != controller)
        {
            animatedSprite.sprite = null; // so the last speaker's frame isn't copied over
            animator.runtimeAnimatorController = controller;
            playedTaggedState = false;

            // Start the new controller now, so its first frame and states are ready this frame.
            if (controller != null)
                animator.Update(0f);
        }

        Sprite still = character != null && character.portrait != null ? character.portrait : defaultPortrait;

        if (controller != null && animatedSprite.sprite != null)
            SetSprite(animatedSprite.sprite);
        else
            SetSprite(still);
    }

    /// <summary>Plays the state named by an "#anim:State" hashtag on the line.</summary>
    private void PlayTaggedState(string[] metadata)
    {
        string state = null;

        if (metadata != null)
            foreach (string tag in metadata)
                if (tag.StartsWith("anim:"))
                    state = tag.Substring("anim:".Length).Trim();

        if (animator.runtimeAnimatorController == null)
        {
            if (state != null)
                Debug.LogWarning($"[PortraitDialogueUI] #anim:{state} needs the speaker to have a Portrait Animator.", this);
            return;
        }

        if (string.IsNullOrEmpty(state))
        {
            // Back to the controller's default state after a line that picked one.
            if (playedTaggedState)
            {
                animator.Rebind();
                animator.Update(0f);
                playedTaggedState = false;
            }
            return;
        }

        int hash = Animator.StringToHash(state);
        if (!animator.HasState(0, hash))
        {
            Debug.LogWarning($"[PortraitDialogueUI] '{animator.runtimeAnimatorController.name}' has no state '{state}'.", this);
            return;
        }

        animator.Play(hash, 0, 0f);
        animator.Update(0f);
        playedTaggedState = true;
    }

    private void SetSprite(Sprite sprite)
    {
        portraitImage.sprite = sprite;
        portraitImage.preserveAspect = true;
        // No sprite would draw a white box.
        portraitImage.enabled = sprite != null;
    }
}
