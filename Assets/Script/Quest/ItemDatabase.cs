using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [SerializeField] private ItemData[] allItems;

    private Dictionary<string, ItemData> itemLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildDatabase();
    }

    private void BuildDatabase()
    {
        itemLookup = new Dictionary<string, ItemData>();

        foreach (var item in allItems)
        {
            if (item == null || string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning("Invalid item entry in database.");
                continue;
            }

            if (itemLookup.ContainsKey(item.itemID))
            {
                Debug.LogWarning("Duplicate item ID: " + item.itemID);
                continue;
            }

            itemLookup.Add(item.itemID, item);
        }
    }

    public ItemData GetItem(string itemID)
    {
        if (itemLookup.TryGetValue(itemID, out ItemData item))
            return item;

        Debug.LogError("Item not found: " + itemID);
        return null;
    }
}