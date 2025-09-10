using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisDetection : DETECTION
{
    public override void Event()
    {
        base.Event();
        GM.Instance.GameManager.isMantisBossFightStarted = true;
    }
}
