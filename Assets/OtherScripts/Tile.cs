using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Tile tile;

    [SerializeField] private Auto_Elevator Elevator1F;
    [SerializeField] private Auto_Elevator Elevator2F;

    protected float stateTimer = 0;

    public BoxCollider2D cd { get; private set; }


    private void Start()
    {
        cd = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        

        if (PlayerManager.Instance.player.transform.position.y -1 > tile.transform.position.y && stateTimer < 0)
        {
            tile.cd.enabled = true;
            Collider2D[] colliders = Physics2D.OverlapBoxAll(cd.bounds.center, cd.bounds.size, 0f);
            foreach (var hit in colliders)
            {
                if (hit.GetComponent<Player>() != null && Input.GetKey(KeyCode.S)  )
                    stateTimer = 0.5f;
            }

        }
        else if (PlayerManager.Instance.player.transform.position.y -1< tile.transform.position.y && stateTimer < 0)
            tile.cd.enabled = false;


        else if (stateTimer > 0)
            tile.cd.enabled = false;



        if (Elevator1F != null && Elevator2F != null)
        {
            if (Elevator1F.Interact || Elevator2F.Interact)
                tile.cd.enabled = false;
            else
                tile.cd.enabled = true;
        }




    }

    protected virtual void OnDrawGizmos()
    {
         
    }
}
