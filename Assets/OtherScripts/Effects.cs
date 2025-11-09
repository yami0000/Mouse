using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Effects : MonoBehaviour
{
    public SpriteRenderer sr;

    [Header("Flash FX")]
    [SerializeField] private Material hitMat;
    private Material originalMat;
    [SerializeField] private bool Special;

    [SerializeField] private Color[] freezeColor;
    [SerializeField] private Color[] igniteColor;
    [SerializeField] private Color[] poisonColor;
    [SerializeField] private Color[] shockColor;

    Color originalcolor;
    private void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMat = sr.material;
        originalcolor = sr.color;


        if (Special)
        {
            sr.material = hitMat;
            hitMat.SetFloat("_Hit", 0f);
        }
    }

    private IEnumerator FlashFX()
    {
        sr.material = hitMat;
        //sr.color = Color.white;

        if (Special)
            hitMat.SetFloat("_Hit", 1f);

        yield return new WaitForSeconds(.2f);

        sr.color = originalcolor;
        sr.material = originalMat;

        if (Special)
            hitMat.SetFloat("_Hit", 0f);
    }

    private void CancelColorChange()
    {
        CancelInvoke();
        sr.color = Color.white;


    }

    public void PoisonFxFor(float _seconds)
    {
        InvokeRepeating("PoisonColorFX", 0, .3f);
        Invoke("CancelColorChange", _seconds);


    }
    public void FreezeFxFor(float _seconds)
    {
        InvokeRepeating("FreezeColorFX", 0, .3f);
        Invoke("CancelColorChange", _seconds);


    }

    public void ShockFxFor(float _seconds)
    {
        InvokeRepeating("ShockColorFX", 0, .3f);
        Invoke("CancelColorChange", _seconds);


    }
    public void IgniteFxFor(float _seconds)
    {
        InvokeRepeating("IgniteColorFX", 0, .3f);
        Invoke("CancelColorChange", _seconds);


    }
    private void IgniteColorFX()
    {
        if (sr.color != igniteColor[0])
            sr.color = igniteColor[0];
        else
            sr.color = igniteColor[1];


    }

    private void PoisonColorFX()
    {
        if (sr.color != poisonColor[0])
            sr.color = poisonColor[0];
        else
            sr.color = poisonColor[1];

    }

    private void FreezeColorFX()
    {
        if (sr.color != freezeColor[0])
            sr.color = freezeColor[0];
        else
            sr.color = freezeColor[1];


    }
    private void ShockColorFX()
    {
        if (sr.color != shockColor[0])
            sr.color = shockColor[0];
        else
            sr.color = shockColor[1];


    }




}