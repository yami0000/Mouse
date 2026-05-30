using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAmmoManager : MonoBehaviour
{
    public static EnemyAmmoManager instance;
    public EnemyAmmo Ammo;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }
}
