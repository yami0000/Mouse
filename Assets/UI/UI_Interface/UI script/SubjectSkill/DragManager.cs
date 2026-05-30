using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class DragManager  :MonoBehaviour
{
    public static DragManager Instance;

    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject dragIconPrefab;

    void Awake()
    {
        Instance = this;
    }

    public Image CreateDragIcon(Sprite sprite)
    {
        GameObject obj = Instantiate(dragIconPrefab, canvas.transform);
        Image img = obj.GetComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false; // So it doesn¡¯t block raycasts
        return img;
    }
}
