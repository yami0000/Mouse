using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Smooth mouse-wheel scrolling with elastic overscroll bounce for the shop entry list.
// Lives on the ScrollArea object (needs an Image with Raycast Target ON to receive
// scroll events - alpha can be 0). Scroll events from child entries bubble up here.
//
// Works even when content is smaller than the viewport: wheel input nudges the list
// slightly and it springs back, like overscroll in most modern applications.
public class UI_ShopScroll : MonoBehaviour, IScrollHandler
{
    [Header("References")]
    [SerializeField] private RectTransform viewport;   // the masked area (RectMask2D)
    [SerializeField] private RectTransform content;    // EntryContainer (pivot & anchor at top)

    [Header("Feel")]
    [SerializeField] private float scrollSpeed = 60f;       // pixels per wheel notch
    [SerializeField] private float smoothing = 12f;         // how fast content chases the target
    [SerializeField] private float maxOverscroll = 60f;     // max pixels past the edge
    [SerializeField] private float overscrollResistance = 0.4f; // input dampening past the edge
    [SerializeField] private float springSpeed = 8f;        // how fast it snaps back in bounds

    private float targetY;
    private float currentY;

    private void OnEnable()
    {
        // Shop just opened - start at the top.
        targetY = 0f;
        currentY = 0f;
        ApplyPosition();
    }

    public void OnScroll(PointerEventData eventData)
    {
        float delta = -eventData.scrollDelta.y * scrollSpeed;

        // Dampen input while already past the edge, so overscroll feels resistant.
        if (targetY < 0f || targetY > MaxScroll())
            delta *= overscrollResistance;

        targetY += delta;

        // Hard limit how far past the edge the target can go.
        targetY = Mathf.Clamp(targetY, -maxOverscroll, MaxScroll() + maxOverscroll);
    }

    private void Update()
    {
        // Spring the target back into the valid range when it's past an edge.
        float clampedTarget = Mathf.Clamp(targetY, 0f, MaxScroll());
        if (!Mathf.Approximately(targetY, clampedTarget))
            targetY = Mathf.Lerp(targetY, clampedTarget, springSpeed * Time.unscaledDeltaTime);

        // Smoothly chase the target.
        currentY = Mathf.Lerp(currentY, targetY, smoothing * Time.unscaledDeltaTime);

        ApplyPosition();
    }

    private float MaxScroll()
    {
        // How far the content can legitimately scroll. 0 when it fits in the viewport.
        return Mathf.Max(0f, content.rect.height - viewport.rect.height);
    }

    private void ApplyPosition()
    {
        Vector2 pos = content.anchoredPosition;
        pos.y = currentY;
        content.anchoredPosition = pos;
    }
}