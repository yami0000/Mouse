using UnityEditor;

/// <summary>Draws a MoveAction with Destination or Offset, whichever its Mode uses.</summary>
[CustomPropertyDrawer(typeof(MoveAction))]
public class MoveActionDrawer : FilteredDrawer
{
    protected override bool Show(SerializedProperty _property, SerializedProperty _child)
    {
        SerializedProperty mode = _property.FindPropertyRelative("mode");
        if (mode.hasMultipleDifferentValues)
            return true;

        switch (_child.name)
        {
            case "destination": return mode.enumValueIndex == (int)MoveAction.Mode.ToTransform;
            case "offset":      return mode.enumValueIndex == (int)MoveAction.Mode.ByOffset;
            default:            return true;
        }
    }
}
