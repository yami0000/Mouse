using System.Collections;
using UnityEngine;

// Fades out a ghost sprite spawned by Boss_Magician.SpawnGhosts()
// and destroys itself when fully transparent.
public class AfterimageGhost : MonoBehaviour
{
    private SpriteRenderer sr;

    public void StartFade(float fadeTime)
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(Fade(fadeTime));
    }

    IEnumerator Fade(float fadeTime)
    {
        Color start = sr.color;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(start.a, 0f, elapsed / fadeTime);
            sr.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }

        Destroy(gameObject);
    }
}