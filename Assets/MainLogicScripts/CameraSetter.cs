using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetter : MonoBehaviour
{
    public static CameraSetter Instance { get; private set; }


    [SerializeField] private CinemachineVirtualCamera _vcam;
   
    private Vector3 current;
    Transform player;

    private void Awake()
    {
        // Setup the Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        player = PlayerManager.Instance.player.transform;
       current = _vcam.transform.position;
        _vcam.Follow = player;
        
    }
    

    public void EnablePosition(Transform T) 
    {
        _vcam.Follow = T;
    }

    public void DisablePosition() 
    {
        _vcam.Follow = player;
    }
}
