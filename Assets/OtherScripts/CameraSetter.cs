using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetter : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _vcam;
    [SerializeField] private Transform MantisPosition;
    private Vector3 current;
    Transform player;
    void Start()
    {
        player = PlayerManager.Instance.player.transform;
       current = _vcam.transform.position;
        _vcam.Follow = player;
        
    }
    private void Update()
    {
        if (GM.Instance.GameManager.isMantisBossFightStarted)
        {
            _vcam.Follow = MantisPosition;
            
        }
        else
            _vcam.Follow = player;
    }
}
