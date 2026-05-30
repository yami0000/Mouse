using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoEffect : MonoBehaviour

{
    [HideInInspector]public Vector2 finalDirection;
    [HideInInspector]public Vector2 lastFramePosition;
    public AnimationCurve damageToParticleCount;
    public GameObject splashEffectPrefab;
    public void _OnDestroy(Vector2 lastVelocity,int Dmg)
    {
        if (splashEffectPrefab != null)
        {

            Vector2 reverseDir = -lastVelocity.normalized;
            float angle = Mathf.Atan2(reverseDir.y, reverseDir.x) * Mathf.Rad2Deg;


            GameObject effect = Instantiate(splashEffectPrefab, transform.position, Quaternion.Euler(angle, -90, 0));
             

            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float countFromCurve = damageToParticleCount.Evaluate(Dmg);
                short finalCount = (short)Mathf.Clamp(countFromCurve, 0, 25);  

                 
                var emission = ps.emission;
                 
                emission.SetBurst(0, new ParticleSystem.Burst(0, finalCount));


                ps.Play();
                Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
            }
        }
    }
    private void FixedUpdate()
    {
        lastFramePosition = transform.position;
    }
}
