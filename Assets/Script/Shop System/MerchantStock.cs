using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach this to each merchant NPC.
// Drag the ItemData assets this merchant sells into itemsForSale in the Inspector.
public class MerchantStock : MonoBehaviour
{
    [Tooltip("Unique ID used by the <<open_shop \"id\">> Yarn command.")]
    public string merchantID;

    public List<ItemData> itemsForSale;

    // Call this from your interaction system (InteractableObject event,
    // Yarn command, NPCAction, etc.) to open the shop with this merchant's stock.
    public void OpenShop()
    {
        UI_Shop.Instance.Open(this);
    }

    /// <summary>
    /// Finds a MerchantStock in the current scene by ID.
    /// Returns null and logs a warning if not found.
    /// </summary>
    public static MerchantStock Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var merchant in FindObjectsByType<MerchantStock>(FindObjectsSortMode.None))
            if (merchant.merchantID == id)
                return merchant;

        Debug.LogWarning($"[MerchantStock] No merchant with ID '{id}' found in scene " +
                         $"'{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'.");
        return null;
    }
}