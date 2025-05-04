using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    private GameObject cam;



    private bool move;

    [SerializeField] private double min;
    [SerializeField] private double max;


    public double Map(double x, double xMin, double xMax, double yMin, double yMax)
    {

        return (x - xMin) * (yMax - yMin) / (xMax - xMin) + yMin;
    }


    public void Awake()
    {
        cam = GameObject.FindWithTag("Virtual Camera");
        Positioning();
    }

    private void Start()
    {




    }

    private void Update()
    {
        Positioning();
    }

    private void Positioning()
    {
        float x = (float)Map(cam.transform.position.x, -17, 8.7, min, max);

        if (-17f < cam.transform.position.x && cam.transform.position.x <8.7)
            move = true;
        else
            move = false;





        if (move)
            transform.position = new Vector3(x, transform.position.y);
        else if (-17f > cam.transform.position.x)
            transform.position = new Vector3((float)min, transform.position.y);
        else if (cam.transform.position.x > 8.7)
            transform.position = new Vector3((float)max, transform.position.y);




    }
}

