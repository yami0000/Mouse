using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Listens for Inventory.OnItemAdded and drives the acquisition toasts.
/// Keeps one toast per distinct ItemData: a repeat acquisition bumps the
/// existing toast's count instead of spawning a new one.
///
/// Stacks the toasts down from the container's top-right corner itself, instead of
/// a layout group, so when one leaves the ones below glide up into its place.
///
/// Assumes this UI lives in the same scene as Inventory (scene-local, created
/// and destroyed together). It subscribes in Start() so Inventory.Awake() has
/// already set Inventory.Instance. If you later make this object persist
/// (DontDestroyOnLoad) while Inventory stays scene-local, you'd need to
/// re-subscribe on scene load - but that's GameManager's job, not this one's.
/// </summary>
public class UI_ItemNotification : MonoBehaviour
{
    public static UI_ItemNotification Instance;

    [SerializeField] private ItemToast toastPrefab;
    [SerializeField] private RectTransform toastContainer; // toasts stack down from its top-right corner
    [SerializeField] private int maxVisible = 4;       // 0 = unlimited

    [Header("Stacking")]
    [Tooltip("Where the toasts' top-right corner sits, from the container's top-right corner. " +
             "x > 0 pushes them right (past the screen edge), y < 0 pushes them down.")]
    [SerializeField] private Vector2 stackOffset = new Vector2(217f, 0f);
    [Tooltip("Gap between toasts.")]
    [SerializeField] private float spacing = 0f;
    [Tooltip("How fast the toasts below glide up into a gap. Higher = faster.")]
    [SerializeField] private float moveSpeed = 10f;

    private readonly Dictionary<ItemData, ItemToast> activeToasts = new Dictionary<ItemData, ItemToast>();
    private readonly List<ItemData> order = new List<ItemData>(); // oldest first, for eviction
    private readonly List<ItemToast> stack = new List<ItemToast>(); // top to bottom, as on screen

    private bool subscribed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // This script places the toasts; a layout group would snap them back every rebuild.
        LayoutGroup layout = toastContainer != null ? toastContainer.GetComponent<LayoutGroup>() : null;
        if (layout != null)
            layout.enabled = false;
    }

    private void Start()
    {
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
        if (Instance == this) Instance = null;
    }

    private void LateUpdate()
    {
        stack.RemoveAll(toast => toast == null);

        // Frame-rate independent ease, unscaled so it still moves while the game is paused.
        float ease = 1f - Mathf.Exp(-moveSpeed * Time.unscaledDeltaTime);
        float slotY = 0f;

        foreach (ItemToast toast in stack)
        {
            toast.StackY = Mathf.Lerp(toast.StackY, slotY, ease);
            PlaceToast(toast);
            slotY += toast.Rect.rect.height + spacing;
        }
    }

    private void Subscribe()
    {
        if (subscribed || Inventory.Instance == null) return;
        Inventory.Instance.OnItemAdded += HandleItemAdded;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || Inventory.Instance == null) return;
        Inventory.Instance.OnItemAdded -= HandleItemAdded;
        subscribed = false;
    }

    private void HandleItemAdded(ItemData item, int count)
    {
        if (item == null || toastPrefab == null || toastContainer == null) return;

        // Same item already on screen -> bump it and keep it alive.
        if (activeToasts.TryGetValue(item, out ItemToast existing) && existing != null)
        {
            existing.AddCount(count);
            order.Remove(item);
            order.Add(item); // freshen so it isn't the next one evicted
            return;
        }

        // Over the cap -> evict the oldest before adding the new one.
        // It slides out first; the new toast waits below it and glides up once it's gone.
        if (maxVisible > 0 && order.Count >= maxVisible)
        {
            ItemData oldest = order[0];
            if (activeToasts.TryGetValue(oldest, out ItemToast oldToast) && oldToast != null)
                oldToast.DismissNow(); // stays in the stack until it has slid out

            // Forget it now, so the next acquisition doesn't pick the same leaving toast again.
            Forget(oldest);
        }

        ItemToast toast = Instantiate(toastPrefab, toastContainer);

        // Every toast hangs from the container's top-right corner, so they all line up the same.
        RectTransform rect = toast.Rect;
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;

        activeToasts[item] = toast;
        order.Add(item);
        toast.Initialize(item, count, OnToastDismissed);

        // Start in the first free slot at the bottom; LateUpdate moves it up as others leave.
        toast.StackY = BottomSlotY();
        stack.Add(toast);
        PlaceToast(toast);
    }

    private void PlaceToast(ItemToast toast)
    {
        toast.Rect.anchoredPosition = new Vector2(stackOffset.x + toast.SlideOffset, stackOffset.y - toast.StackY);
    }

    private float BottomSlotY()
    {
        float slotY = 0f;
        foreach (ItemToast toast in stack)
            if (toast != null)
                slotY += toast.Rect.rect.height + spacing;
        return slotY;
    }

    private void OnToastDismissed(ItemToast toast)
    {
        stack.Remove(toast);

        // A toast evicted and replaced for the same item mustn't take the new one's entry with it.
        if (activeToasts.TryGetValue(toast.Data, out ItemToast current) && current == toast)
            Forget(toast.Data);
    }

    private void Forget(ItemData item)
    {
        activeToasts.Remove(item);
        order.Remove(item);
    }
}
