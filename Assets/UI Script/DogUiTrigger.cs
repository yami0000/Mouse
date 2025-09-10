using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogUiTrigger : MonoBehaviour
{
    [SerializeField] private Auto_Elevator Elevator;

    private void Update()
    {
        if (Elevator.Interact)
        {
            FindAnyObjectByType<UI_Dog>().OpenMenu();
            Elevator.Interact = false;  
        }
    }
}
