using UnityEngine;

/// <summary>One speaker in the CharacterDatabase: the portrait shown while they talk.</summary>
[System.Serializable]
public class CharacterProfile
{
    // First, so the Inspector names each list element after it.
    [Tooltip("The name before the colon in Yarn lines (\"Grandma: Hello!\"). Not case sensitive.")]
    public string characterName;
    [Tooltip("Still portrait. Also shown for a moment while the Portrait Animator starts.")]
    public Sprite portrait;
    [Tooltip("Optional. Plays this Animator Controller instead of the still portrait. Its clips can be the " +
             "character's world animations (they animate a SpriteRenderer's sprite). " +
             "\"#anim:State\" at the end of a Yarn line plays that state for the line.")]
    public RuntimeAnimatorController portraitAnimator;
}
