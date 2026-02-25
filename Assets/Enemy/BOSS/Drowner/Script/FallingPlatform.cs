using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum Num
{
  First,
  Second,
  Third


}
public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private Boss_Drowner boss;

    [Header("shaking")]
    [SerializeField] float duration;
    [SerializeField] float magnitude;
    [Header("Falling")]
    [SerializeField] float Y;
    [SerializeField] float fallDuration;
    [Header("recover")]
    [SerializeField] float CD;
    [SerializeField] float recoverDuration;

    private Vector3 StartPos;
    private int LastStanding;
    public Num num;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (num == Num.First)
                boss.Num = 1;
            else if (num == Num.Second)
                boss.Num = 2;
            else
                boss.Num = 3;


        }
    }
    
    public void Falling() 
    {
       StartPos = transform.position;
        StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        boss.isFalling = true;  
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float damper = 1f - (elapsed / duration);

            float x = Random.Range(-1f, 1f) * magnitude * damper;
            float y = Random.Range(-1f, 1f) * magnitude * damper;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = StartPos;

        Vector3 diveStart = StartPos;
        Vector3 diveEnd = new Vector3(transform.position.x, Y, transform.position.z);

        elapsed = 0f;
        while (elapsed < fallDuration)
        {
            float t = elapsed / fallDuration;
            t = Mathf.SmoothStep(0, 1, t);  
            transform.position = Vector3.Lerp(diveStart, diveEnd, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = diveEnd;

        yield return new WaitForSeconds(CD);

        
        Vector3 UPEnd = new Vector3(transform.position.x,StartPos.y , transform.position.z);

        elapsed = 0f;
        while (elapsed < recoverDuration)
        {
            float t = elapsed / recoverDuration; 
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(transform.position, UPEnd, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = UPEnd;
        boss.isFalling = false;

    }

}
