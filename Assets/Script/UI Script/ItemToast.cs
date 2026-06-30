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
/// Prefab hierarchy expected:
///   Root            (layout-controlled by the container's VerticalLayoutGroup; fixed height)
///     SlideRoot     (RectTransform that this script slides horizontally)
///       Icon  (Image)
///       Name  (TextMeshProUGUI)
///       Count (TextMeshProUGUI)
///
/// We animate SlideRoot, NOT the root, so the layout group can stack toasts
/// vertically without fighting the horizontal slide.
/// </summary>
public class ItemToast : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform slideRoot;
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
    private float restingX;
    private bool shown;
    private Coroutine lifecycle;
    private Action<ItemData> onDismiss;

    public void Initialize(ItemData item, int amount, Action<ItemData> dismissCallback)
    {
        data = item;
        count = amount;
        onDismiss = dismissCallback;

        restingX = slideRoot.anchoredPosition.x;

        itemImage.sprite = item.icon;
        itemImage.preserveAspect = true;
        nameText.text = item.itemName;
        RefreshCount();

        // start parked off-screen to the right
        slideRoot.anchoredPosition = new Vector2(restingX + hiddenOffsetX, slideRoot.anchoredPosition.y);

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
            yield return SlideTo(restingX, slideInDuration, EaseOutBack);
            shown = true;
        }

        yield return new WaitForSecondsRealtime(holdDuration);
        yield return ExitRoutine();
    }

    private IEnumerator ExitRoutine()
    {
        yield return SlideTo(restingX + hiddenOffsetX, slideOutDuration, EaseInCubic);
        onDismiss?.Invoke(data);
        Destroy(gameObject);
    }

    private IEnumerator SlideTo(float targetX, float duration, Func<float, float> ease)
    {
        float startX = slideRoot.anchoredPosition.x;
        float elapsed = 0f;

        // unscaled so notifications still animate while the game is paused (Time.timeScale = 0)
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float x = Mathf.LerpUnclamped(startX, targetX, ease(t));
            slideRoot.anchoredPosition = new Vector2(x, slideRoot.anchoredPosition.y);
            yield return null;
        }
        slideRoot.anchoredPosition = new Vector2(targetX, slideRoot.anchoredPosition.y);
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
