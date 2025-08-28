using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Ammo Effect", menuName = "Data/Ammo/Shotgun Ammo")]
public class Effect_ShotgunAmmo : ItemEffect
{
    [SerializeField] private GameObject AmmoPrefab;
    [SerializeField] private float xVelocity;
    [SerializeField] private float Range;
    [SerializeField] private float speed;
    [SerializeField] private int pelletCount;
    [SerializeField] private float spreadAngle;

    private void Start()
    {
        PlayerManager.Instance.player.Timer = speed;
    }

    public override void ExecuteEffect(Transform _position)
    {
        if (PlayerManager.Instance.player.Timer > 0)
            return;

        Player player = PlayerManager.Instance.player;

        
        for (int i = 0; i < pelletCount; i++)
        {
            // Pick a random angle within ¡À(spreadAngle/2)
            float angle = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);

            // Convert angle to direction vector
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right * player.facingDir;

            
            GameObject Ammo = Instantiate(AmmoPrefab, player.transform.position, Quaternion.identity);
            Ammo.GetComponent<Rigidbody2D>().velocity = direction.normalized * xVelocity;

             
            Destroy(Ammo, Range);
        }

        PlayerManager.Instance.player.Timer = speed;
    }
}
