using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponHolder : MonoBehaviour
{

    public Rigidbody2D rb { get; private set; }

    public static PlayerWeaponHolder Instance;

    public GameObject previousWeapon;

    

    [SerializeField] private GameObject equipmentPrefab; 

    private void Start()
    {
        

    }

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        else
            Instance = this;
    }
    public void EquipWeapon(ItemData_Equipment weaponData)
    {
        Destroy(previousWeapon);
        
         

        Transform player = PlayerManager.Instance.player.transform;

        GameObject EquippedWeapon = Instantiate(equipmentPrefab, player.position, player.rotation);

        EquippedWeapon.GetComponent<EquipmentObject>().SetUpEquipment(weaponData);

        previousWeapon = EquippedWeapon;




    }

    public void Update()
    {
        
        
        if (previousWeapon == null)
            return;


        Transform transforms = this.previousWeapon.transform;

        Transform player = PlayerManager.Instance.player.transform;
        transforms.position = new Vector2(player.position.x,player.position.y-0.9f);

        

        if (PlayerManager.Instance.player.facingDir == -1)
            transforms.rotation = Quaternion.Euler(0,180,0); 
        else if(PlayerManager.Instance.player.facingDir == 1)
            transforms.rotation = Quaternion.Euler(0, 0, 0);

       
     }



}