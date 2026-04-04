using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitboxs : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private EnemyStats enemyStats;

    [Header("Hitboxes")]
     
    [SerializeField] private Collider2D[] hitboxes;

    private bool hasDealtDamage = false;
    
    private List<GameObject> hitTargets = new List<GameObject>();

    private void Start()
    {
        if (enemyStats == null) enemyStats = GetComponentInParent<EnemyStats>();
        DisableAllHitboxes();
    }

   
    public void EnableHitbox(int index)
    {
        if (index < 0 || index >= hitboxes.Length) return;

        hitboxes[index].enabled = true;
        hasDealtDamage = false;  
        hitTargets.Clear();      
    }

    public void DisableHitbox(int index)
    {
        if (index < 0 || index >= hitboxes.Length) return;
        hitboxes[index].enabled = false;
    }

    public void DisableAllHitboxes()
    {
        foreach (var hb in hitboxes)
        {
            hb.enabled = false;
        }
    }
     private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.TryGetComponent(out PlayerStats player) && !hitTargets.Contains(collision.gameObject))
        {
            hitTargets.Add(collision.gameObject);
            enemyStats.DoDamage(player);

          
        }
    }
}
