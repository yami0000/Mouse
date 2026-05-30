using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public Collider2D cd { get; private set; }
     public EntityStats stats { get; private set; } 
    [SerializeField] private GameObject explosionEffectPrefab;
    private void Start()
    {
        cd = GetComponent<Collider2D>();
        stats = GetComponent<EntityStats>();
        

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy")) 
        {
            Debug.Log("Explode");
            Explode();
            Destroy(gameObject);
        }
           
    }

    private void Explode()
    {
        GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();

        Explode_Grenade G = effect.GetComponent<Explode_Grenade>();
        G.Initialize(stats);

        if (ps != null)
        {
            Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(effect);
        }
 
       
    }
}
