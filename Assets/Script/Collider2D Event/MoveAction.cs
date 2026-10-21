using System.Collections;
using UnityEngine;

/// <summary>
/// Moves one object from wherever it is to a Transform, or by an offset, over a set time.
/// Used by CameraEvent steps, consequences and InteractableSwitch doors. The movement itself
/// runs on a TransformMover added to the moving object, so it keeps going whoever started it.
/// </summary>
[System.Serializable]
public class MoveAction
{
    public enum Mode
    {
        ToTransform,
        ByOffset,
    }

    [Tooltip("The object that moves, e.g. a door.")]
    public Transform target;

    [Tooltip("To Transform = end on the Destination's position.\nBy Offset = move this far from where the object is when the move starts.")]
    public Mode mode = Mode.ToTransform;

    [Tooltip("Where the object ends up. Its Z is ignored, the object keeps its own.")]
    public Transform destination;

    [Tooltip("How far the object moves, in world units.")]
    public Vector2 offset;

    [Tooltip("Seconds the move takes. 0 = snap there at once.")]
    [Min(0)] public float duration = 1f;

    [Tooltip("Ease in and out instead of moving at a constant speed.")]
    public bool smooth = true;

    public bool IsMoving => target != null && target.TryGetComponent(out TransformMover mover) && mover.IsMoving;

    public Vector3 GetDestination(Vector3 _from)
    {
        if (mode == Mode.ByOffset)
            return _from + (Vector3)offset;

        if (destination == null)
            return _from;

        return new Vector3(destination.position.x, destination.position.y, _from.z);
    }

    /// <summary>Starts moving and returns at once.</summary>
    public void Begin()
    {
        if (target != null)
            Begin(GetDestination(target.position));
    }

    /// <summary>Starts moving to a given point instead of the configured one, e.g. back to where a door started.</summary>
    public void Begin(Vector3 _destination)
    {
        if (target != null)
            TransformMover.For(target).MoveTo(_destination, duration, smooth);
    }

    /// <summary>Moves and waits until the object arrives.</summary>
    public IEnumerator Run()
    {
        Begin();

        while (IsMoving)
            yield return null;
    }
}
