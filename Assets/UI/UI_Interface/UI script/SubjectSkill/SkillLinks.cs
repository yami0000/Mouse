using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws every line between joints as one mesh, in the editor as well, so no line ever has to be placed
/// by hand. Sits first under the joints' parent, so the lines stay behind them.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class SkillLinks : MaskableGraphic
{
    [SerializeField, Min(0)] private float width = 6f;
    [SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color activeColor = new Color(0.45f, 0.85f, 1f);
    [Tooltip("Stop lines at the joint's edge, so they don't show through a see-through locked joint.")]
    [SerializeField] private bool stopAtJointEdge = true;

    // One mesh across the whole map: judged by this object's own rect it would be clipped as a single
    // piece, and vanish whole when that rect pans out of view. The mask's clip rect still cuts the lines.
    public override void Cull(Rect clipRect, bool validRect) { }

#if UNITY_EDITOR
    // Follow joints being moved around while editing. In play the mesh only changes with the tree.
    private void Update()
    {
        if (!Application.isPlaying)
            SetVerticesDirty();
    }
#endif

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (transform.parent == null) return;

        bool playing = Application.isPlaying;

        foreach (SkillJoint joint in transform.parent.GetComponentsInChildren<SkillJoint>(true))
        {
            if (playing && !joint.IsVisible) continue;

            foreach (SkillJoint earlier in joint.previous)
            {
                if (earlier == null || (playing && !earlier.IsVisible)) continue;

                Color lineColor = playing && !(earlier.OpensNext && joint.IsActive) ? lockedColor : activeColor;
                AddLine(vh, earlier, joint, lineColor);
            }
        }
    }

    private void AddLine(VertexHelper _vh, SkillJoint _from, SkillJoint _to, Color32 _color)
    {
        Vector2 from = rectTransform.InverseTransformPoint(_from.transform.position);
        Vector2 to = rectTransform.InverseTransformPoint(_to.transform.position);

        Vector2 along = to - from;
        float length = along.magnitude;
        if (length < 0.01f) return;
        along /= length;

        if (stopAtJointEdge)
        {
            float fromEdge = Radius(_from);
            float toEdge = Radius(_to);

            if (fromEdge + toEdge >= length) return; // the joints overlap: nothing to draw between them

            from += along * fromEdge;
            to -= along * toEdge;
        }

        Vector2 side = new Vector2(-along.y, along.x) * (width * 0.5f);
        int first = _vh.currentVertCount;

        _vh.AddVert(from - side, _color, Vector4.zero);
        _vh.AddVert(from + side, _color, Vector4.zero);
        _vh.AddVert(to + side, _color, Vector4.zero);
        _vh.AddVert(to - side, _color, Vector4.zero);

        _vh.AddTriangle(first, first + 1, first + 2);
        _vh.AddTriangle(first + 2, first + 3, first);
    }

    private static float Radius(SkillJoint _joint)
    {
        RectTransform rect = (RectTransform)_joint.transform;
        return Mathf.Min(rect.rect.width * rect.localScale.x, rect.rect.height * rect.localScale.y) * 0.5f;
    }
}
