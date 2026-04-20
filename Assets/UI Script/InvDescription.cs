using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InvDescription : MonoBehaviour
{
    [SerializeField] private RectTransform backgroundRect;
   // [SerializeField] private TextMeshProUGUI Name;
    //[SerializeField] private TextMeshProUGUI Genre;
    //[SerializeField] private TextMeshProUGUI Effect;
   // [SerializeField] private TextMeshProUGUI Damage;

    public Transform contentContainer; // The object with the Grid Layout Group
    public GameObject textPrefab;// Your TMP text prefab
    [SerializeField] private float One;
    [SerializeField] private float OneTwo;
    [SerializeField] private float TwoThree;
    [SerializeField] private float ThreeFour;

    private void Start()
    {
        Hide();
    }
    void Update()
    {
        // Follow the mouse
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            Input.mousePosition,
            null,
            out position);
        transform.localPosition = position + new Vector2(10, -10); // offset
    }

    public void Show(InventoryItem item, ItemData_Equipment equip)
    {
        
        gameObject.SetActive(true);

        // 1. Clear previous info
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        
        List<string> infoLines = new List<string>();

        CreateTextElement(item.data.itemName, 16f, true,One);

    

         
        string typeStr = equip == null ? item.data.ItemType.ToString() : equip.equipmentType.ToString();
        CreateTextElement(typeStr, 8f,false,OneTwo);


        if (equip == null) 
        CreateTextElement(item.data.Description, 8f, false, TwoThree);
        if (equip != null)
        {
            float dmg = (equip.FirePower + equip.Damage) / Mathf.Max(0.0001f, equip.firingRate);
            CreateTextElement($"<b><size=100%>{(int)dmg}</size></b> <size=50%>DPS</size>", 20f,false,TwoThree);
            CreateTextElement(item.data.Description, 8f, false, ThreeFour);
        }
    }

    private void CreateTextElement(string content, float fontSize, bool isBold, float cellHeight)
    {
        GameObject newText = Instantiate(textPrefab, contentContainer);
        TextMeshProUGUI tmp = newText.GetComponent<TextMeshProUGUI>();

        LayoutElement layout = newText.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.preferredHeight = cellHeight;
            
        }

        tmp.text = content;
        tmp.fontSize = fontSize;

        if (isBold)
            tmp.fontStyle = FontStyles.Bold;
    }

    public void Hide()
    {
        if (gameObject == null) return;
        gameObject.SetActive(false);
    }
}
