 
using System.Collections;
using System.Collections.Generic;
 
using UnityEngine;
using static UnityEditor.Search.SearchColumn;




[System.Serializable]
public class DropEntry
{
    public ItemData item;
    [Range(0, 100)] public int dropChance = 100;

    // For items that can drop multiple times
    public int rollTimes = 1;
}



public class ItemDrop : MonoBehaviour
{
   


    [SerializeField] private DropEntry[] dropTable;
    [SerializeField] private GameObject dropPrefab;

    public void GenerateDrop()
    {
        foreach (var entry in dropTable)
        {
            for (int i = 0; i < entry.rollTimes; i++)
            {
                if (Random.Range(0, 100) <= entry.dropChance)
                {
                    DropItem(entry.item);
                }
            }
        }
    }



    public void DropItem(ItemData _itemData)
    {
     GameObject newDrop = Instantiate(dropPrefab,transform.position,Quaternion.identity);

    Vector2 randomVelocity = new Vector2(Random.Range(-5,5), Random.Range(10,15));   
    
    newDrop.GetComponent<ItemDropMovement>().SetUpItem(_itemData, randomVelocity);
    
    }
}
