using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] Enemy_Medic enemy;
    [SerializeField] private float AnimationTime;
    public ParticleSystem Heal;
    

    private void Start()
    {
        Heal.Stop();

    }

    private void Update()
    {
        if(enemy._enemy != null)
            transform.position = enemy._enemy.transform.position;
    }

    public void ExcuteHealingFX() 
    {
        
        StartCoroutine(HealingFX());

    }

    IEnumerator HealingFX() 
    {
        float t = 0f;
        Heal.Play();
        while(t < AnimationTime)
        {
             
            
            t += Time.deltaTime;
            yield return null;
           
        }

        t= 0f;
        Heal.Stop() ;   


    
    
    }
}
