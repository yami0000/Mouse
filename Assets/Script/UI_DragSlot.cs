using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Drag and drop between UI slots. Dragging a slot sends a copy of its icon along under the pointer;
/// dropping it on another slot calls Receive() on that slot, which decides what the move means
/// (install, swap, equip...). A drop anywhere else does nothing.
/// Any slot that can be dragged from or dropped onto derives from this; the two ends of a drag
/// don't have to be the same class, so every future drag between slots goes through here.
/// </summary>
public abstract class UI_DragSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    /// <summary>The slot being dragged right now, or null.</summary>
    public static UI_DragSlot Dragging { get; private set; }

    private Image dragIcon;

    /// <summary>
    /// The image dragged along: its sprite, size and scale are copied. Null, or an image without a
    /// sprite, and this slot can't be dragged (an empty slot, or one that only takes drops).
    /// </summary>
    protected abstract Image DragImage { get; }

    /// <summary>Called on the slot something was dropped on. Do the move here; true when it was taken.</summary>
    protected virtual bool Receive(UI_DragSlot _from) => false;

    /// <summary>Called on the dragged slot as its drag starts, e.g. to hide its tooltip or dim its icon.</summary>
    protected virtual void OnDragStarted() { }

    /// <summary>Called on the dragged slot once its drag is over, after any Receive().</summary>
    protected virtual void OnDragEnded() { }

    public void OnBeginDrag(PointerEventData _event)
    {
        Image source = DragImage;
        Canvas canvas = GetComponentInParent<Canvas>();

        if (source == null || source.sprite == null || canvas == null)
        {
            // Nothing to drag: with no drag object, Unity sends no Drag, Drop or EndDrag for this press.
            _event.pointerDrag = null;
            return;
        }

        dragIcon = CreateIcon(source, canvas.rootCanvas);
        Dragging = this;
        OnDragStarted();
        Follow(_event);
    }

    public void OnDrag(PointerEventData _event)
    {
        if (dragIcon != null)
            Follow(_event);
    }

    // Unity sends the target's OnDrop before the dragged slot's OnEndDrag.
    public void OnEndDrag(PointerEventData _event) => EndDrag();

    public void OnDrop(PointerEventData _event)
    {
        UI_DragSlot from = Dragging;
        if (from != null && from != this)
            Receive(from);
    }

    protected virtual void OnDisable()
    {
        // e.g. the menu closed mid-drag: no EndDrag will come, so clean up here.
        if (Dragging == this)
            EndDrag();
    }

    private void EndDrag()
    {
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);

        dragIcon = null;

        if (Dragging != this) return;

        Dragging = null;
        OnDragEnded();
    }

    private void Follow(PointerEventData _event)
    {
        RectTransform canvasRect = (RectTransform)dragIcon.canvas.transform;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, _event.position, _event.pressEventCamera, out Vector3 world))
            dragIcon.rectTransform.position = world;
    }

    /// <summary>A copy of _source on top of everything in _canvas, the same size on screen, that never blocks the drop.</summary>
    private static Image CreateIcon(Image _source, Canvas _canvas)
    {
        GameObject go = new GameObject("Drag Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = _source.gameObject.layer;

        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(_canvas.transform, false);
        rect.SetAsLastSibling();
        rect.sizeDelta = _source.rectTransform.rect.size;

        Vector3 sourceScale = _source.rectTransform.lossyScale;
        Vector3 canvasScale = _canvas.transform.lossyScale;
        rect.localScale = new Vector3(sourceScale.x / canvasScale.x, sourceScale.y / canvasScale.y, 1f);

        Image icon = go.GetComponent<Image>();
        icon.sprite = _source.sprite;
        // Its own proportions, like the slot shows it (slots keep theirs too).
        icon.preserveAspect = true;
        icon.color = Color.white;
        // The slot under the pointer has to get the drop, not the icon.
        icon.raycastTarget = false;
        return icon;
    }
}
