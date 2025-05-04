using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] LineRenderer line;
    [SerializeField] Animator animator;
    [SerializeField] Detection detection;
    [SerializeField] LayerMask WallsLayer;
    [SerializeField] Enemy_Laser enemy;

    //[SerializeField] float chargingwidth;
    [SerializeField] float firingwidth;

    [SerializeField] Gradient chargingColor;
    [SerializeField] Gradient firingColor;

    public Vector2 laserDirection;
    public float laserLenth;
    private void Start()
    {   
        line.SetPosition(1, Vector3.zero);
       

    }
    private void Update()
    {
        //ShowChargeLaser();
        line.widthMultiplier = firingwidth;
    }
    public void ShowChargeLaser( ) 
    {
        ShowLaser();
        animator.Play("Laser");
        line.colorGradient = chargingColor;
    }

    public void FireLaser()
    {
        
        line.colorGradient = firingColor;
    }
    public void HideLaser()
    {

        line.SetPosition(1, Vector3.zero);
        
    }

    private void ShowLaser()
    {
        
        laserDirection = detection.DirectionToPlayer();

        RaycastHit2D hitWall = Physics2D.Raycast(transform.position, laserDirection, float.MaxValue, WallsLayer); 

       
        laserLenth = (transform.position - ((Vector3)hitWall.point)).magnitude;

        Vector3 targetpos = new Vector3
        (
            laserLenth,
            0f,
            0f
        );

       

        line.SetPosition(1, targetpos);

        Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * detection.DirectionToPlayer();
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, rotatedVectorToTarget);
        transform.rotation = targetRotation;
    }


}
