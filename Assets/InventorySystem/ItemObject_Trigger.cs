using UnityEngine;

public class ItemObject_Trigger : MonoBehaviour
{
    private ItemDropMovement myItemObject;

    private void Awake() => myItemObject = GetComponentInParent<ItemDropMovement>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            myItemObject.PickUpItem();

        }

    }
}
