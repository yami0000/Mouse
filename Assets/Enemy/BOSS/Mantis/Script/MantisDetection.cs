using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantisDetection : DETECTION
{
    public override void Event()
    {
        base.Event();
        if (GM.Instance.GameManager.isMantisAlive)
        {
            GM.Instance.GameManager.isMantisBossFightStarted = true;
            CameraSetter.Instance.EnablePosition(this.transform);
        }
        else
        {
            GM.Instance.GameManager.isMantisBossFightStarted = false;
            CameraSetter.Instance.DisablePosition();
        }

       
    }
}
