using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// The window onto the skill map, on the object whose rect is the visible area. Drag anywhere in it, joints
/// included, to move the map; the wheel zooms around the cursor. Drags that begin on a joint move the map
/// instead of activating it, since the EventSystem cancels the click once a drag starts.
/// How far the map moves comes from where the joints are, so nothing needs resizing as the tree grows.
/// </summary>
public class UI_SkillMapView : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler, IPointerClickHandler
{
    [Tooltip("Holds the joints: moved and scaled by this script. A direct child of this object.")]
    [SerializeField] private RectTransform content;
    [SerializeField] private RectMask2D mask;

    [Header("Zoom")]
    [SerializeField, Min(0.05f)] private float minZoom = 0.4f;
    [SerializeField, Min(0.05f)] private float maxZoom = 1.6f;
    [SerializeField, Min(0.05f)] private float startZoom = 1f;
    [Tooltip("How much one wheel notch changes the zoom. 0.1 = 10%.")]
    [SerializeField, Range(0.01f, 0.5f)] private float zoomStep = 0.1f;

    [Header("Moving")]
    [Tooltip("How far past the outermost joints the map can be moved.")]
    [SerializeField, Min(0)] private float padding = 200f;

    private RectTransform view;
    private Rect jointBounds;
    private Vector2 lastPointer;
    private bool opened;

    private float Zoom => content.localScale.x;

    private void Awake()
    {
        view = (RectTransform)transform;

        // Off in the saved scene, so the whole map can be seen and edited in the Scene view.
        if (mask != null)
            mask.enabled = true;
    }

    private void OnEnable()
    {
        MeasureJoints();

        if (!opened)
        {
            opened = true;
            ResetView();
        }
    }

    /// <summary>The middle of all the joints in the middle of the window, at the start zoom.</summary>
    public void ResetView()
    {
        content.localScale = Vector3.one * Mathf.Clamp(startZoom, minZoom, maxZoom);
        content.anchoredPosition = -jointBounds.center * Zoom; // Joints is anchored to the window's middle
        ClampToBounds();
    }

    // Every joint counts, the hidden ones too, so the map's limits don't jump when cyberware goes in.
    private void MeasureJoints()
    {
        SkillJoint[] joints = content.GetComponentsInChildren<SkillJoint>(true);

        if (joints.Length == 0)
        {
            jointBounds = new Rect(-padding, -padding, padding * 2f, padding * 2f);
            return;
        }

        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;

        foreach (SkillJoint joint in joints)
        {
            RectTransform rect = (RectTransform)joint.transform;
            Vector2 centre = content.InverseTransformPoint(rect.position);
            Vector2 half = Vector2.Scale(rect.rect.size, rect.localScale) * 0.5f;

            min = Vector2.Min(min, centre - half);
            max = Vector2.Max(max, centre + half);
        }

        min -= Vector2.one * padding;
        max += Vector2.one * padding;
        jointBounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SkillTree.Instance != null)
            SkillTree.Instance.Description.Hide();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(view, eventData.position, eventData.pressEventCamera, out lastPointer);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(view, eventData.position, eventData.pressEventCamera, out Vector2 pointer))
            return;

        content.anchoredPosition += pointer - lastPointer;
        lastPointer = pointer;
        ClampToBounds();
    }

    public void OnScroll(PointerEventData eventData)
    {
        float notch = Mathf.Sign(eventData.scrollDelta.y);
        if (Mathf.Approximately(eventData.scrollDelta.y, 0f)) return;

        float oldZoom = Zoom;
        float newZoom = Mathf.Clamp(oldZoom * (1f + notch * zoomStep), minZoom, maxZoom);
        if (Mathf.Approximately(oldZoom, newZoom)) return;

        // The point of the map under the cursor has to stay under it.
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(content, eventData.position, eventData.enterEventCamera, out Vector2 pointOnMap))
            return;

        content.localScale = Vector3.one * newZoom;
        content.anchoredPosition += pointOnMap * (oldZoom - newZoom);
        ClampToBounds();
    }

    // A click on the empty map closes the cyberware list. Clicks on joints don't get here.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && SkillTree.Instance != null)
            SkillTree.Instance.CloseCyberwarePanel();
    }

    /// <summary>Stops the map leaving the window. Along an axis where all of it fits, it just sits in the middle.</summary>
    private void ClampToBounds()
    {
        Rect window = view.rect;
        Vector2 origin = view.InverseTransformPoint(content.position);
        Vector2 min = origin + jointBounds.min * Zoom;
        Vector2 max = origin + jointBounds.max * Zoom;

        content.anchoredPosition += new Vector2(
            Correction(min.x, max.x, window.xMin, window.xMax),
            Correction(min.y, max.y, window.yMin, window.yMax));
    }

    private static float Correction(float _min, float _max, float _windowMin, float _windowMax)
    {
        if (_max - _min <= _windowMax - _windowMin)
            return (_windowMin + _windowMax - _min - _max) * 0.5f;

        if (_min > _windowMin) return _windowMin - _min;
        if (_max < _windowMax) return _windowMax - _max;
        return 0f;
    }
}
