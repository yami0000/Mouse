using System.Collections.Generic;
using System.Linq;
using UnityEditor;

/// <summary>
/// Draws an EventAction (or a ConsequenceStep) with only the settings of the consequences that are
/// ticked, so a camera-only event is not buried under reward and quest fields it never uses.
/// </summary>
[CustomPropertyDrawer(typeof(EventAction), true)]
public class EventActionDrawer : FilteredDrawer
{
    private static readonly Dictionary<string, Consequences> Groups = new Dictionary<string, Consequences>
    {
        { "battleTarget",    Consequences.EnterBattleMode },
        { "rewards",         Consequences.DropReward },
        { "rewardOrigin",    Consequences.DropReward },
        { "burstSideways",   Consequences.DropReward },
        { "burstUp",         Consequences.DropReward },
        { "questToAccept",   Consequences.AcceptQuest },
        { "objectiveKey",    Consequences.ProgressQuest },
        { "questToComplete", Consequences.CompleteQuest },
        { "move",            Consequences.Move },
        { "dialogueName",    Consequences.Dialogue },
    };

    // ConsequenceStep timing first, then what happens.
    protected override string[] Lead => new[] { "delay", "runWithNext", "consequences" };

    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        SerializedProperty flags = _property.FindPropertyRelative("consequences");

        // A multi selection that disagrees shows everything rather than guessing.
        if (flags.hasMultipleDifferentValues)
            return true;

        return !Groups.TryGetValue(_child.name, out Consequences needs) || ((Consequences)flags.intValue & needs) != 0;
    }

    protected override string Summary(SerializedProperty _property)
    {
        var ticked = (Consequences)_property.FindPropertyRelative("consequences").intValue;

        string what = ticked == Consequences.None
            ? "Nothing"
            : string.Join(", ", ticked.ToString().Split(',').Select(n => ObjectNames.NicifyVariableName(n.Trim())));

        if ((ticked & Consequences.Move) != 0)
            what += $" ({NameOf(_property.FindPropertyRelative("move.target"), "no target")})";

        if ((ticked & Consequences.Dialogue) != 0)
        {
            string node = _property.FindPropertyRelative("dialogueName").stringValue;
            what += $" ({(string.IsNullOrEmpty(node) ? "no node" : node)})";
        }

        return what + Timing(_property);
    }
}
