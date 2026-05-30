using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class FastTravelAni : UI_NPC
{
    [Header("Panel Ani")]
    public RectTransform panel;
    private Vector2 _Start;
    public Vector2 End; 
    public float duration = 0.5f;
    public AnimationCurve curve;
    [Header("Rotate")]
    public RectTransform Pole;
    public float firstTargetZ;
    public float firstDuration = 0.5f;
    public AnimationCurve firstCurve; 
    public float pingPongAmount = 2f;
    public float pingPongSpeed = 2f;

    [Header("Floating")]
    public RectTransform sign;
    private Vector2 _Start1;
    public Vector2 End1;
    public float duration1 = 0.6f;
    public AnimationCurve curve1;
    private Vector2 centerPos;
    public float radius = 3f;
    public float speed = 1f;
    [Header("Floating for another")]
    public RectTransform sign2;
    private Vector2 _Start2;
    public Vector2 End2;
    private Vector2 centerPos2;
    public float radius2 = 3f;
    public float speed2 = 1f;




    private float baseZ;


    private void Awake()
    {
        _Start = panel.anchoredPosition;
        baseZ = Pole.localEulerAngles.z;
        _Start1 = sign.anchoredPosition;
        _Start2 = sign2.anchoredPosition; 
         
    }
    public override void CloseMenu()
    {
        base.CloseMenu();
        panel.position = _Start;

        Vector3 angles = Pole.localEulerAngles;
        angles.z = baseZ;
        Pole.localEulerAngles = angles;

        sign.anchoredPosition = _Start1;
        sign2.anchoredPosition = _Start2;

       
    }

    public override void OpenMenu()
    {
        base.OpenMenu();

        SlideIn(End);

        FloatIn(End1);

        FloatIn2(End2);

        StartCoroutine(RotateSequence());

    }

    #region PanelAni
    public void SlideIn(Vector2 targetPos)
    {
        StartCoroutine(MoveUI(_Start, targetPos));
    }

    IEnumerator MoveUI(Vector2 start, Vector2 end)
    {
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            float curvedT = curve.Evaluate(t);

            panel.anchoredPosition = Vector2.Lerp(start, end, curvedT);

            yield return null;
        }

        panel.anchoredPosition = end;
    }

    #endregion

    #region
    IEnumerator RotateSequence()
    {
        // -------- Phase 1: Curve rotation --------
        float startZ = baseZ;
        float time = 0;

        while (time < firstDuration)
        {
            time += Time.deltaTime;
            float t = time / firstDuration;

            float curvedT = firstCurve.Evaluate(t);
            float z = Mathf.Lerp(startZ, firstTargetZ, curvedT);

            Pole.localRotation = Quaternion.Euler(0, 0, z);

            yield return null;
        }

        Pole.localRotation = Quaternion.Euler(0, 0, firstTargetZ);
        

        // -------- Phase 2: Endless PingPong --------
        while (GM.Instance.GameManager.isUIOpened)
        {
            float zOffset = Mathf.Sin(Time.time * pingPongSpeed)
                  * (pingPongAmount / 2f);

            Pole.localRotation = Quaternion.Euler(0, 0, firstTargetZ + zOffset);

            yield return null;
        }
    }
    #endregion

    public void FloatIn(Vector2 targetPos)
    {
        StartCoroutine(FloatUI(_Start1, targetPos));
    }
    IEnumerator FloatUI(Vector2 start, Vector2 end)
    {
        float time = 0;

        while (time < duration1)
        {
            time += Time.deltaTime;
            float t = time / duration1;

            float curvedT = curve1.Evaluate(t);

            sign.anchoredPosition = Vector2.Lerp(start, end, curvedT);

            yield return null;
        }

        centerPos = End1;
        time = 0f;
        while (GM.Instance.GameManager.isUIOpened)
        {
            time += Time.deltaTime;

            float x = Mathf.Cos(time * speed) * radius;
            float y = Mathf.Sin(time * speed) * radius;

            sign.anchoredPosition = centerPos + new Vector2(x, y);

            yield return null;

        }
    }

    public void FloatIn2(Vector2 targetPos)
    {
        StartCoroutine(FloatUI2(_Start2, targetPos));
    }
    IEnumerator FloatUI2(Vector2 start, Vector2 end)
    {
        float time = 0;

        while (time < duration1)
        {
            time += Time.deltaTime;
            float t = time / duration1;

            float curvedT = curve1.Evaluate(t);

            sign2.anchoredPosition = Vector2.Lerp(start, end, curvedT);

            yield return null;
        }

        centerPos2 = End2;
        time = 0f;
        while (GM.Instance.GameManager.isUIOpened)
        {
            time += Time.deltaTime;

            float x = Mathf.Sin(time * speed2) * radius2;
            float y = Mathf.Cos(time * speed2) * radius2;

            sign2.anchoredPosition = centerPos2 + new Vector2(x, y);

            yield return null;

        }
    }
}
