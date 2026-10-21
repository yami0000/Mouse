using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Description : MonoBehaviour
{
    

    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;

    private bool isShowing = false;

    private void Start()
    {
        // Start() runs on the first activation, which can be the Show() call itself,
        // so only hide if nothing has asked to show the tooltip yet.
        if (!isShowing)
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

    public void Show(string skillName, string description)
    {
        isShowing = true;
        gameObject.SetActive(true);
        skillNameText.text = skillName;
        skillDescriptionText.text = description;
    }

    public void Hide()
    {
        isShowing = false;
        gameObject.SetActive(false);
    }
}
