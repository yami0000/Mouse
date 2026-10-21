using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Pivot")]
    [Tooltip("The transform that rotates. Place it at the turret's rotation center and make the barrel sprite its child, pointing RIGHT. Leave empty to rotate this object.")]
    [SerializeField] private Transform pivot;

    [Header("Angle Range (0 = right, positive = counter-clockwise)")]
    [SerializeField] private float minAngle = -45f;
    [SerializeField] private float maxAngle = 10f;

    [Header("Idle")]
    [Tooltip("A: shortest wait before picking a new random angle")]
    [SerializeField] private float minInterval = 1f;
    [Tooltip("B: longest wait before picking a new random angle")]
    [SerializeField] private float maxInterval = 3f;
    [SerializeField] private float idleRotateSpeed = 60f;

    [Header("Tracking")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float trackRotateSpeed = 180f;
    [Tooltip("Offset from the player's position to aim at (e.g. aim at the chest instead of the feet)")]
    [SerializeField] private Vector2 targetOffset;

    private Transform player;
    private float currentAngle;
    private float idleTargetAngle;
    private float idleTimer;
    private bool isTracking;

    private void Awake()
    {
        if (pivot == null)
            pivot = transform;
    }

    private void Start()
    {
        currentAngle = Mathf.Clamp(Mathf.DeltaAngle(0f, pivot.localEulerAngles.z), minAngle, maxAngle);
        idleTargetAngle = currentAngle;
        ResetIdleTimer();
        ApplyAngle();
    }

    private void Update()
    {
        if (player == null && PlayerManager.Instance != null && PlayerManager.Instance.player != null)
            player = PlayerManager.Instance.player.transform;

        if (TryGetPlayerAngle(out float playerAngle))
        {
            isTracking = true;
            currentAngle = Mathf.MoveTowards(currentAngle, playerAngle, trackRotateSpeed * Time.deltaTime);
        }
        else
        {
            // Player left: go back to the idle angle we had before tracking, then resume the idle cycle
            if (isTracking)
            {
                isTracking = false;
                ResetIdleTimer();
            }
            UpdateIdle();
        }

        ApplyAngle();
    }

    private void UpdateIdle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            idleTargetAngle = Random.Range(minAngle, maxAngle);
            ResetIdleTimer();
        }

        currentAngle = Mathf.MoveTowards(currentAngle, idleTargetAngle, idleRotateSpeed * Time.deltaTime);
    }

    private bool TryGetPlayerAngle(out float angle)
    {
        angle = 0f;
        if (player == null)
            return false;

        Vector2 worldOffset = (Vector2)player.position + targetOffset - (Vector2)pivot.position;
        if (worldOffset.sqrMagnitude > detectRange * detectRange)
            return false;

        // Convert into the pivot's parent space so a rotated / flipped (scale.x < 0) turret still works
        Vector2 localOffset = pivot.parent != null ? (Vector2)pivot.parent.InverseTransformVector(worldOffset) : worldOffset;
        angle = Mathf.Atan2(localOffset.y, localOffset.x) * Mathf.Rad2Deg;

        return angle >= minAngle && angle <= maxAngle;
    }

    private void ResetIdleTimer() => idleTimer = Random.Range(minInterval, maxInterval);

    private void ApplyAngle() => pivot.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

    private void OnValidate()
    {
        if (maxAngle < minAngle)
            maxAngle = minAngle;
        minInterval = Mathf.Max(0f, minInterval);
        if (maxInterval < minInterval)
            maxInterval = minInterval;
        detectRange = Mathf.Max(0f, detectRange);
    }

    private void OnDrawGizmosSelected()
    {
        Transform p = pivot != null ? pivot : transform;
        Vector3 center = p.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + AngleToWorldDir(p, minAngle) * detectRange);
        Gizmos.DrawLine(center, center + AngleToWorldDir(p, maxAngle) * detectRange);
    }

    private static Vector3 AngleToWorldDir(Transform p, float angle)
    {
        Vector3 local = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
        return p.parent != null ? p.parent.TransformVector(local).normalized : local;
    }
}
