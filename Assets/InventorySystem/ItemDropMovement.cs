using UnityEngine;

public class ItemDropMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ItemData itemData;

    private bool pickedUp;

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
        // Destroy only happens at the end of the frame, so a second collider touching the
        // pickup in the same frame would otherwise collect it twice.
        if (pickedUp) return;
        pickedUp = true;

        Inventory.Instance.AddItem(itemData);
        GameEvents.OnItemCollected?.Invoke(itemData, 1);
        Destroy(gameObject);

    }
}
