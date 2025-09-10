using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class UI_Mantis : MonoBehaviour
{   
    [SerializeField] GameObject Mantis1;
    [SerializeField] GameObject Mantis2;
    [SerializeField] GameObject Health;
    [SerializeField] GameObject MantisHP;
 
     
    bool UIshowed;
    private void Start()
    {
         
        Mantis1.SetActive(false);
        Mantis2.SetActive(false);
        MantisHP.SetActive(false);
        UIshowed = true;
    }
    private void Update()
    {
        

        if (GM.Instance.GameManager.isMantisBossFightStarted)
        {
            if (UIshowed)
            {
                UIshowed = false;
                StartCoroutine(ShowIntro());
            }


        }
    }

    IEnumerator ShowIntro()
    {
        float M = 0f;

        
        Health.SetActive(false);
        
        while (!PlayerManager.Instance.player.IsGroundDetected())
        {
            //M += Time.deltaTime;
            yield return null;
        }

       // M = 0f;
        GM.Instance.GameManager.isUIOpened = true;
        while (M < 5.5f) 
        {
         M += Time.deltaTime; 
　　　　 yield return null;
        } 
        
        M= 0f;

        
        Mantis1.SetActive (true);
        while(M<2f)
        {
            M += Time.deltaTime;
            yield return null;
        }

        Mantis2.SetActive (true);
        while (M < 4f)
        {
            M += Time.deltaTime;
            yield return null;
        }

        M = 0f;

        GM.Instance.GameManager.isUIOpened = false;
        Health.SetActive (true);
        Mantis1.SetActive(false);
        Mantis2.SetActive(false);
        MantisHP.SetActive (true);


    }
}
