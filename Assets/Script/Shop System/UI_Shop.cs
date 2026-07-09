using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Shop : MonoBehaviour
{
    public static UI_Shop Instance;

    [Header("Panel")]
    [SerializeField] private GameObject uiParent;          // the whole shop page (child of Canvas)

    [Header("Left Side - Entry List")]
    [SerializeField] private GameObject entryPrefab;       // project asset (UI_ShopEntry on it)
    [SerializeField] private Transform entryContainer;     // LIVE hierarchy object with VerticalLayoutGroup

    [Header("Right Side - Info Panel")]
    [SerializeField] private TextMeshProUGUI infoNameText;
    [SerializeField] private TextMeshProUGUI infoTypeText;
    [SerializeField] private TextMeshProUGUI infoDescriptionText;
    [SerializeField] private Image infoIconImage;

    [Header("Top Right - Currency")]
    [SerializeField] private TextMeshProUGUI coinText;

    private readonly List<UI_ShopEntry> spawnedEntries = new List<UI_ShopEntry>();
    private bool isOpen;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        uiParent.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyUp(KeyCode.Escape))
            Close();
    }

    public void Open(MerchantStock merchant)
    {
        if (isOpen) return;
        isOpen = true;

        uiParent.SetActive(true);
        GM.Instance.GameManager.isUIOpened = true;

        BuildEntries(merchant);
        RefreshCoinText();

        // Show the first item's info by default
        if (merchant.itemsForSale.Count > 0)
            ShowInfo(merchant.itemsForSale[0]);
        else
            ClearInfo();
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        uiParent.SetActive(false);
        GM.Instance.GameManager.isUIOpened = false;
    }

    private void BuildEntries(MerchantStock merchant)
    {
        foreach (UI_ShopEntry entry in spawnedEntries)
            Destroy(entry.gameObject);
        spawnedEntries.Clear();

        foreach (ItemData item in merchant.itemsForSale)
        {
            if (item == null) continue;

            GameObject newEntry = Instantiate(entryPrefab, entryContainer);
            UI_ShopEntry shopEntry = newEntry.GetComponent<UI_ShopEntry>();
            shopEntry.Setup(item, this);
            spawnedEntries.Add(shopEntry);
        }
    }

    #region Info Panel

    public void ShowInfo(ItemData item)
    {
        if (item == null) return;

        infoNameText.text = item.itemName;
        infoTypeText.text = item.ItemType.ToString();
        infoDescriptionText.text = item.Description;

        infoIconImage.sprite = item.icon;
        infoIconImage.preserveAspect = true;
        infoIconImage.color = Color.white;
    }

    private void ClearInfo()
    {
        infoNameText.text = "";
        infoTypeText.text = "";
        infoDescriptionText.text = "";
        infoIconImage.sprite = null;
        infoIconImage.color = Color.clear;
    }

    #endregion

    #region Purchase

    public void TryPurchase(ItemData item)
    {
        ItemData currency = GetCurrency();
        int coins = GetCurrencyCount(currency);

        if (coins < item.price)
        {
            Debug.Log("You can't afford " + item.name);
            return;
        }

        if (currency != null)
        {
            for (int i = 0; i < item.price; i++)
                Inventory.Instance.RemoveItem(currency);
        }

        Inventory.Instance.AddItem(item);
        Debug.Log("Purchased " + item.name);

        RefreshCoinText();
    }

    private ItemData GetCurrency()
    {
        foreach (KeyValuePair<ItemData, InventoryItem> pair in Inventory.Instance.stashDictionary)
        {
            if (pair.Key.ItemType == ItemType.Currency)
                return pair.Key;
        }
        return null;
    }

    private int GetCurrencyCount(ItemData currency)
    {
        if (currency == null) return 0;

        if (Inventory.Instance.stashDictionary.TryGetValue(currency, out InventoryItem value))
            return value.stackSize;

        return 0;
    }

    private void RefreshCoinText()
    {
        coinText.text = GetCurrencyCount(GetCurrency()).ToString("D6"); // 000402 style
    }

    #endregion
}