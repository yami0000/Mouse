using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listens for Inventory.OnItemAdded and drives the acquisition toasts.
/// Keeps one toast per distinct ItemData: a repeat acquisition bumps the
/// existing toast's count instead of spawning a new one.
///
/// Assumes this UI lives in the same scene as Inventory (scene-local, created
/// and destroyed together). It subscribes in Start() so Inventory.Awake() has
/// already set Inventory.Instance. If you later make this object persist
/// (DontDestroyOnLoad) while Inventory stays scene-local, you'd need to
/// re-subscribe on scene load ¡ª but that's GameManager's job, not this one's.
/// </summary>
public class UI_ItemNotification : MonoBehaviour
{
    public static UI_ItemNotification Instance;

    [SerializeField] private ItemToast toastPrefab;
    [SerializeField] private Transform toastContainer; // VerticalLayoutGroup, anchored to the right edge
    [SerializeField] private int maxVisible = 4;       // 0 = unlimited

    private readonly Dictionary<ItemData, ItemToast> activeToasts = new Dictionary<ItemData, ItemToast>();
    private readonly List<ItemData> order = new List<ItemData>(); // oldest first, for eviction

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
        if (maxVisible > 0 && order.Count >= maxVisible)
        {
            ItemData oldest = order[0];
            if (activeToasts.TryGetValue(oldest, out ItemToast oldToast) && oldToast != null)
                oldToast.DismissNow(); // OnToastDismissed cleans up the bookkeeping
            else
                OnToastDismissed(oldest);
        }

        ItemToast toast = Instantiate(toastPrefab, toastContainer);
        activeToasts[item] = toast;
        order.Add(item);
        toast.Initialize(item, count, OnToastDismissed);
    }

    private void OnToastDismissed(ItemData item)
    {
        activeToasts.Remove(item);
        order.Remove(item);
    }
}
