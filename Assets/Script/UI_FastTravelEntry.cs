using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// One fast travel row, created by UI_FastTravelList. Passes the mouse to the list,
/// which decides what is highlighted, and draws itself lit or not.
/// </summary>
public class UI_FastTravelEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private UI_FastTravelList list;
    private int index;
    private Image background;
    private TextMeshProUGUI label;
    private Color normalBackground, highlightedBackground;
    private Color normalText, highlightedText;

    public void Setup(UI_FastTravelList owner, int rowIndex, Image bg, TextMeshProUGUI text,
                      Color bgNormal, Color bgHighlighted, Color textNormal, Color textHighlighted)
    {
        list = owner;
        index = rowIndex;
        background = bg;
        label = text;
        normalBackground = bgNormal;
        highlightedBackground = bgHighlighted;
        normalText = textNormal;
        highlightedText = textHighlighted;

        SetHighlighted(false);
    }

    public void SetHighlighted(bool on)
    {
        background.color = on ? highlightedBackground : normalBackground;
        label.color = on ? highlightedText : normalText;
    }

    public void OnPointerEnter(PointerEventData eventData) => list.OnRowEntered(index);

    public void OnPointerExit(PointerEventData eventData) => list.OnRowExited(index);

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right-click closes the menu (UI_NPC.Update), so only the left button travels.
        if (eventData.button != PointerEventData.InputButton.Left) return;
        list.OnRowClicked(index);
    }
}
