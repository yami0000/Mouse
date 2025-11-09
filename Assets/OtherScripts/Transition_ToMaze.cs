using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_ToMaze : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {


        if (collision.GetComponent<Player>() != null)
        {

            SceneController.instance.ToBugRegion();
        }

    }
}
