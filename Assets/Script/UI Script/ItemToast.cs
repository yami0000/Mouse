using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single "item acquired" toast. Owns its own bounce-in / hold / bounce-out
/// coroutine so its lifetime is independent of Inventory. Re-acquiring the same
/// item bumps the count and resets the hold timer (see AddCount).
///
/// UI_ItemNotification places the toast: it stacks them and glides the ones below
/// up when one leaves. This script only animates SlideOffset (how far right of its
/// slot the toast is), which the manager adds to the x, so the two never fight over
/// the position.
/// </summary>
public class ItemToast : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;

    [Header("Animation")]
    [SerializeField] private float slideInDuration = 0.35f;
    [SerializeField] private float slideOutDuration = 0.25f;
    [SerializeField] private float holdDuration = 2.5f;   // your "t seconds"
    [SerializeField] private float hiddenOffsetX = 400f;  // how far off-screen right to park it

    private ItemData data;
    private int count;
    private bool shown;
    private Coroutine lifecycle;
    private Action<ItemToast> onDismiss;

    public ItemData Data => data;
    public RectTransform Rect => (RectTransform)transform;

    /// <summary>How far right of its slot the toast is: hiddenOffsetX parked off screen, 0 shown.</summary>
    public float SlideOffset { get; private set; }

    /// <summary>How far down the stack the toast is right now. Eased towards its slot by UI_ItemNotification.</summary>
    public float StackY { get; set; }

    public void Initialize(ItemData item, int amount, Action<ItemToast> dismissCallback)
    {
        data = item;
        count = amount;
        onDismiss = dismissCallback;

        itemImage.sprite = item.icon;
        itemImage.preserveAspect = true;
        nameText.text = item.itemName;
        RefreshCount();

        // start parked off-screen to the right
        SlideOffset = hiddenOffsetX;

        RestartLifecycle();
    }

    /// <summary>Called when the same item is acquired again while this toast is alive.</summary>
    public void AddCount(int amount)
    {
        count += amount;
        RefreshCount();
        RestartLifecycle(); // resets the hold timer so it lingers
    }

    /// <summary>Force an immediate exit (used by the manager when evicting the oldest toast).</summary>
    public void DismissNow()
    {
        if (lifecycle != null) StopCoroutine(lifecycle);
        lifecycle = StartCoroutine(ExitRoutine());
    }

    private void RefreshCount()
    {
        countText.text = count > 1 ? "x" + count : "";
    }

    private void RestartLifecycle()
    {
        if (lifecycle != null) StopCoroutine(lifecycle);
        lifecycle = StartCoroutine(LifecycleRoutine());
    }

    private IEnumerator LifecycleRoutine()
    {
        if (!shown)
        {
            yield return SlideTo(0f, slideInDuration, EaseOutBack);
            shown = true;
        }

        yield return new WaitForSecondsRealtime(holdDuration);
        yield return ExitRoutine();
    }

    private IEnumerator ExitRoutine()
    {
        yield return SlideTo(hiddenOffsetX, slideOutDuration, EaseInCubic);
        onDismiss?.Invoke(this);
        Destroy(gameObject);
    }

    private IEnumerator SlideTo(float targetOffset, float duration, Func<float, float> ease)
    {
        float startOffset = SlideOffset;
        float elapsed = 0f;

        // unscaled so notifications still animate while the game is paused (Time.timeScale = 0)
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SlideOffset = Mathf.LerpUnclamped(startOffset, targetOffset, ease(t));
            yield return null;
        }
        SlideOffset = targetOffset;
    }

    // slight overshoot on entry for the "bounce" feel
    private static float EaseOutBack(float t)
    {
        const float s = 1.70158f;
        t -= 1f;
        return t * t * ((s + 1f) * t + s) + 1f;
    }

    private static float EaseInCubic(float t) => t * t * t;
}
