using UnityEngine;

/// <summary>
/// Something the player presses E at, inside this object's trigger Collider2D.
///
/// Door: opens the door if it is closed and closes it if it is open. Closed = where the door sits
/// when the scene starts, open = the Door move's Destination / Offset. With Remember State the door
/// is put back the way it was left when the scene loads again (kept in GameManager's world flags).
/// Call Elevator: brings the elevator to the stop nearest this switch, unless it is already there.
/// </summary>
public class InteractableSwitch : DETECTION
{
    public enum Mode
    {
        Door,
        CallElevator,
    }

    [SerializeField] private Mode mode = Mode.Door;

    [Tooltip("Target = the door. Destination / Offset = where it goes when opened.")]
    [SerializeField] private MoveAction door = new MoveAction();

    [Tooltip("Keep the door open / closed as it was left, even after leaving the scene and coming back.")]
    [SerializeField] private bool rememberState = true;

    [Tooltip("Once opened it stays open: the switch no longer closes it. E.g. a sealed door opened from the inside.")]
    [SerializeField] private bool openOnlyOnce = false;

    [Tooltip("Name the door's state is saved under. Empty = scene name + the door's place in the hierarchy, " +
             "so switches on both sides of the same door share it. Set one if the door gets renamed or moved in the hierarchy.")]
    [SerializeField] private string saveId;

    [SerializeField] private Elevator elevator;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool open;

    private void Start()
    {
        if (door.target == null) return;

        closedPosition = door.target.position;
        openPosition = door.GetDestination(closedPosition);

        // Put back the way it was left, at once rather than played again.
        if (mode == Mode.Door && rememberState && LoadOpen())
        {
            open = true;
            TransformMover.For(door.target).MoveTo(openPosition, 0f, false);
        }
    }

    public override void Interact()
    {
        base.Interact();

        if (mode == Mode.Door)
            ToggleDoor();
        else if (elevator != null)
            elevator.Call(transform.position);
    }

    private void ToggleDoor()
    {
        if (door.target == null) return;

        // When the door is still, go by where it actually is, in case something else (a CameraEvent) moved it.
        if (!door.IsMoving)
        {
            Vector3 position = door.target.position;
            open = (position - openPosition).sqrMagnitude < (position - closedPosition).sqrMagnitude;
        }

        if (open && openOnlyOnce) return;

        open = !open;
        door.Begin(open ? openPosition : closedPosition);

        if (rememberState)
            SaveOpen(open);
    }

    #region Saved state

    private string Key
    {
        get
        {
            if (!string.IsNullOrEmpty(saveId)) return "Door:" + saveId;

            Transform target = door.target;
            string path = target.name;
            for (Transform t = target.parent; t != null; t = t.parent)
                path = t.name + "/" + path;

            return "Door:" + target.gameObject.scene.name + ":" + path;
        }
    }

    private bool LoadOpen() => GameManager.Instance != null && GameManager.Instance.GetFlag(Key);

    private void SaveOpen(bool _open)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetFlag(Key, _open);
    }

    #endregion
}
