using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Ammo Effect", menuName = "Data/Enemy Item/SelfDes")]
public class SelfDes : MonoBehaviour
{
    [SerializeField] public GameObject AmmoPrefab;
    [SerializeField]private Enemy_SelfDes enemy;

    public void ExecuteEffect_Explode(Transform _position)
    {
        Player player = PlayerManager.Instance.player;

        


        GameObject Explode = Instantiate(AmmoPrefab,enemy.transform.position , Quaternion.identity );
         






        Destroy(Explode, 0.85f);


    }
}
