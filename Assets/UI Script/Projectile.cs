using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public GameObject AmmoPrefab;

    [SerializeField] private Enemy enemy;


    [SerializeField] float Range;
    //[SerializeField] float speed;

    public float launchAngle;

    private Transform player;


 
    public void ExecuteEffect(Transform transform) 
    {
        launchAngle = Random.Range(30f, 75f);
        player = PlayerManager.Instance.player.transform;
        int facingDir = enemy.facingDir;

        Vector2 startPos = transform.position;
        Vector2 targetPos = player.position;

        float dx = Mathf.Abs(targetPos.x - startPos.x);
        float dy = targetPos.y - startPos.y;

        Rigidbody2D rbPrefab = AmmoPrefab.GetComponent<Rigidbody2D>();
        float g = Mathf.Abs(Physics2D.gravity.y * rbPrefab.gravityScale);
        float angleRad = launchAngle * Mathf.Deg2Rad;

        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        float vSqr = (g * dx * dx) / (2f * cos * cos * (dx * Mathf.Tan(angleRad) - dy));

        if (vSqr <= 0)
        {
            Debug.Log("Target not reachable at this angle");
            return;
        }

        float v = Mathf.Sqrt(vSqr);

        // Direction: ensure projectile faces right way (left or right)
        Vector2 velocity = new Vector2(v * cos * facingDir, v * sin);

        GameObject Ammo = Instantiate(AmmoPrefab, startPos, Quaternion.identity);

        Ammo.GetComponent<Rigidbody2D>().velocity = velocity;

        EnemyAmmo_Effect _Ammo = Ammo.GetComponent<EnemyAmmo_Effect>();
        if (_Ammo != null)
            _Ammo.Initialize(enemy);

        Destroy(Ammo, Range);
    }
}
