using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
 private static GameObject[] persistObjects = new GameObject[2];
    public int objectindex;

    private void Awake()
    {
        if (persistObjects[objectindex] == null)
        {
            Debug.Log("0");
            persistObjects[objectindex] = gameObject;
            DontDestroyOnLoad(gameObject);

        }

        else if (persistObjects[objectindex] != gameObject)
        {
            Debug.Log("1" + gameObject);
            Destroy(gameObject);
        }   
    }
}
