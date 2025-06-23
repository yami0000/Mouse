using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;



public class Button_Purchase : MonoBehaviour, IPointerDownHandler
{




    [SerializeField] public ItemData item;

    public void OnPointerDown(PointerEventData eventData)
    {
        
        CheckCurrency();
       
    }
    private void CheckCurrency()
    {
        ItemData currency = null;

        foreach (KeyValuePair<ItemData, InventoryItem> item in Inventory.Instance.stashDictionary)
        {
            if (item.Key.ItemType == ItemType.Currency)
                 currency = item.Key;

        }
        
        
        purchase(currency);
    }

    private void purchase(ItemData currency) 
    {
        if (currency == null)
        {
            Debug.Log("You can't afford " + item.name);
            return;
        }
    if (Inventory.Instance.stashDictionary.TryGetValue(currency, out InventoryItem value)) 
        {
          if(value.stackSize >= item.price) 
            {
            
            Inventory.Instance.AddItem(item);

            for(int i = 0; i < item.price; i++)
            Inventory.Instance.RemoveItem(currency);

                Debug.Log("Purchased " + item.name);
            }
          else
                Debug.Log("You can't afford " + item.name);
        }
    }
}
