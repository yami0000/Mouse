using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeControl : MonoBehaviour
{
    [Header("Pupils")]
    public RectTransform leftPupil;
    public RectTransform rightPupil;

    [Header("Movement Bounds")]
    public Vector2 movementBounds;
    public float smoothness;

    void Update()
    {
         
        Vector2 leftSocketPos = RectTransformUtility.WorldToScreenPoint(null, leftPupil.parent.position);
        Vector2 rightSocketPos = RectTransformUtility.WorldToScreenPoint(null, rightPupil.parent.position);

         
        Vector2 sharedMidpoint = (leftSocketPos + rightSocketPos) / 2f;

         
        float deltaX = Input.mousePosition.x - sharedMidpoint.x;
        float deltaY = Input.mousePosition.y - sharedMidpoint.y;

        
        float normX = 0;
        float normY = 0;

        if (deltaX > 0)
            normX = deltaX / (Screen.width - sharedMidpoint.x);
        else if (deltaX < 0)
            normX = deltaX / sharedMidpoint.x;

        if (deltaY > 0)
            normY = deltaY / (Screen.height - sharedMidpoint.y);
        else if (deltaY < 0)
            normY = deltaY / sharedMidpoint.y;
 
        Vector2 targetAnchoredPos = new Vector2(
            normX * movementBounds.x,
            normY * movementBounds.y
        );
 
        leftPupil.anchoredPosition = Vector2.Lerp(
            leftPupil.anchoredPosition,
            targetAnchoredPos,
            Time.deltaTime * smoothness
        );

        rightPupil.anchoredPosition = Vector2.Lerp(
            rightPupil.anchoredPosition,
            targetAnchoredPos,
            Time.deltaTime * smoothness
        );
    }
}
