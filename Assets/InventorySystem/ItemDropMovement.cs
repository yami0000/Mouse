using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ItemData itemData;
    [SerializeField] private Vector2 velocity;


 
    private void SetUpVisuals()
    {
        if (itemData == null)
            return;

        GetComponent<SpriteRenderer>().sprite = itemData.icon;
        gameObject.name = "Item object -" + itemData.itemName;
    }

 

    public void SetUpItem(ItemData _itemData,Vector2 _velocity)
    {
    itemData = _itemData;
    rb.velocity = _velocity;

        SetUpVisuals();
    }
    public void PickUpItem()
    {
        Inventory.Instance.AddItem(itemData);
        GameEvents.OnItemCollected?.Invoke(itemData, 1);
        Destroy(gameObject);

    }
}
