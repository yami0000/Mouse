using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetter : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _vcam;
    void Start()
    {
        _vcam.Follow = PlayerManager.Instance.player.transform;
    }

}
