using UnityEngine;

public class Wiggle : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] bool wiggleRotation = true;
    [SerializeField] float angle = 10f;
    [SerializeField] float rotationSpeed = 3f;

    [Header("Position")]
    [SerializeField] bool wigglePosition = false;
    [SerializeField] Vector2 offset = new Vector2(0.05f, 0.05f);
    [SerializeField] float positionSpeed = 4f;

    [Header("Scale")]
    [SerializeField] bool wiggleScale = false;
    [SerializeField] float scaleAmount = 0.05f;
    [SerializeField] float scaleSpeed = 5f;

    [Header("Style")]
    [Tooltip("On = smooth random wiggle, Off = regular back-and-forth sway")]
    [SerializeField] bool useNoise = false;
    [Tooltip("Random time offset so multiple objects don't wiggle in sync")]
    [SerializeField] bool randomStart = true;

    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;
    private float seed;

    private void Awake()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
        startScale = transform.localScale;
        seed = randomStart ? Random.Range(0f, 100f) : 0f;
    }

    private void OnDisable()
    {
        // put the object back where it started
        transform.localPosition = startPos;
        transform.localRotation = startRot;
        transform.localScale = startScale;
    }

    private void Update()
    {
        float time = Time.time + seed;

        if (wiggleRotation)
        {
            float z = Wave(time * rotationSpeed, 0f) * angle;
            transform.localRotation = startRot * Quaternion.Euler(0, 0, z);
        }

        if (wigglePosition)
        {
            float x = Wave(time * positionSpeed, 10f) * offset.x;
            float y = Wave(time * positionSpeed, 20f) * offset.y;
            transform.localPosition = startPos + new Vector3(x, y, 0);
        }

        if (wiggleScale)
        {
            float s = 1f + Wave(time * scaleSpeed, 30f) * scaleAmount;
            transform.localScale = startScale * s;
        }
    }

    // returns a value between -1 and 1
    private float Wave(float t, float channel)
    {
        if (useNoise)
            return Mathf.PerlinNoise(t, channel) * 2f - 1f;

        return Mathf.Sin(t + channel);
    }
}
