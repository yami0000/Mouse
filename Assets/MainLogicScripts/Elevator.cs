using System.Collections;
using UnityEngine;

/// <summary>
/// A platform that travels between two stops, A and B.
///
/// Riding: put a trigger Collider2D on this object (next to the solid one the player stands on).
/// When the player is in it, the elevator goes to the other stop, either right away (Auto Ride)
/// or when E is pressed.
/// Calling: an InteractableSwitch set to Call Elevator brings it to the stop nearest the switch.
///
/// Give the platform a Kinematic Rigidbody2D so it carries and pushes the player properly.
/// Without one it is moved by its transform, as before.
/// </summary>
public class Elevator : DETECTION
{
    [Tooltip("First stop. The platform's own position is ignored, it only ever rests on A or B.")]
    [SerializeField] private Transform A;
    [Tooltip("Second stop.")]
    [SerializeField] private Transform B;

    [SerializeField] private float maxSpeed = 3f;
    [Tooltip("How quickly it speeds up and slows down.")]
    [SerializeField] private float acceleration = 5f;
    [Tooltip("Snaps onto the stop once this close.")]
    [SerializeField] private float stopDistance = 0.05f;

    [Tooltip("On = the elevator leaves as soon as the player steps on. Off = the player presses E.")]
    [SerializeField] private bool autoRide = true;

    public bool IsMoving { get; private set; }

    protected override bool ShowPrompt => !autoRide;

    private Rigidbody2D rb;

    private Vector2 Position => rb != null ? rb.position : (Vector2)transform.position;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    #region Riding

    protected override void OnPlayerEnter()
    {
        // Only on entering, so a rider who arrives isn't sent straight back. Step off and on again to go back.
        if (autoRide)
            Ride();
    }

    public override void Interact()
    {
        if (!autoRide)
            Ride();
    }

    /// <summary>Goes to the stop it isn't at.</summary>
    public void Ride()
    {
        if (IsMoving) return;

        GoTo(NearestStop(Position) == A ? B : A);
    }

    #endregion

    #region Calling

    /// <summary>
    /// Brings the elevator to the stop nearest the caller. Returns false if it is already there
    /// or busy moving.
    /// </summary>
    public bool Call(Vector2 _callerPosition)
    {
        Transform stop = NearestStop(_callerPosition);

        if (IsMoving || IsAt(stop))
            return false;

        GoTo(stop);
        return true;
    }

    public Transform NearestStop(Vector2 _point)
    {
        return ((Vector2)A.position - _point).sqrMagnitude <= ((Vector2)B.position - _point).sqrMagnitude ? A : B;
    }

    public bool IsAt(Transform _stop) => !IsMoving && Vector2.Distance(Position, _stop.position) <= stopDistance;

    #endregion

    #region Movement

    private void GoTo(Transform _stop)
    {
        if (_stop == null) return;

        StartCoroutine(MoveTo(_stop.position));
    }

    private IEnumerator MoveTo(Vector2 _target)
    {
        IsMoving = true;
        float speed = 0f;

        while (Vector2.Distance(Position, _target) > stopDistance)
        {
            float dt = rb != null ? Time.fixedDeltaTime : Time.deltaTime;
            float distance = Vector2.Distance(Position, _target);

            // Fastest speed it can still brake from in time, so it eases into the stop instead of overshooting.
            float targetSpeed = Mathf.Min(maxSpeed, Mathf.Sqrt(2f * acceleration * distance));
            speed = Mathf.MoveTowards(speed, targetSpeed, acceleration * dt);

            SetPosition(Vector2.MoveTowards(Position, _target, Mathf.Max(speed, 0.1f) * dt), true);

            yield return rb != null ? new WaitForFixedUpdate() : null;
        }

        SetPosition(_target, false);
        IsMoving = false;
    }

    private void SetPosition(Vector2 _position, bool _interpolate)
    {
        if (rb != null && _interpolate)
        {
            rb.MovePosition(_position);
            return;
        }

        if (rb != null)
            rb.position = _position;

        transform.position = new Vector3(_position.x, _position.y, transform.position.z);
    }

    private void OnDisable()
    {
        // The coroutine dies with the object, so don't stay stuck as "moving".
        IsMoving = false;
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (A == null || B == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(A.position, B.position);
        Gizmos.DrawWireSphere(A.position, 0.25f);
        Gizmos.DrawWireSphere(B.position, 0.25f);
    }
}
