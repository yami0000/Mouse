using System.Collections;
using UnityEngine;

/// <summary>
/// Added at runtime by MoveAction to the object being moved. One move at a time: a new move
/// replaces the old one, so a door told to open while it is still closing turns straight around.
/// Uses Rigidbody2D.MovePosition when the object has a Rigidbody2D (make it Kinematic), so it
/// pushes the player instead of passing through.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("")]
public class TransformMover : MonoBehaviour
{
    private Coroutine routine;
    private Rigidbody2D rb;

    public bool IsMoving => routine != null;

    public static TransformMover For(Transform _target)
    {
        return _target.TryGetComponent(out TransformMover mover) ? mover : _target.gameObject.AddComponent<TransformMover>();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveTo(Vector3 _destination, float _duration, bool _smooth)
    {
        Stop();

        // A coroutine can't start on an inactive object, so just put it there.
        if (_duration <= 0f || !isActiveAndEnabled)
        {
            SetPosition(_destination);
            return;
        }

        routine = StartCoroutine(Move(_destination, _duration, _smooth));
    }

    public void Stop()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = null;
    }

    private IEnumerator Move(Vector3 _destination, float _duration, bool _smooth)
    {
        Vector3 start = transform.position;
        float time = 0f;

        while (time < _duration)
        {
            yield return rb != null ? new WaitForFixedUpdate() : null;

            time += rb != null ? Time.fixedDeltaTime : Time.deltaTime;
            float progress = Mathf.Clamp01(time / _duration);

            if (_smooth)
                progress = Mathf.SmoothStep(0f, 1f, progress);

            SetPosition(Vector3.Lerp(start, _destination, progress), true);
        }

        SetPosition(_destination);
        routine = null;
    }

    private void SetPosition(Vector3 _position, bool _interpolate = false)
    {
        if (rb != null && _interpolate)
        {
            rb.MovePosition(_position);
            return;
        }

        if (rb != null)
            rb.position = _position;

        transform.position = _position;
    }

    private void OnDisable()
    {
        // Coroutines die with the object, clear the handle so IsMoving doesn't stay true forever.
        routine = null;
    }
}
