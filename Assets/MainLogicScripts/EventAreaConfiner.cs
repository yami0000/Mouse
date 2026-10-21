using Cinemachine;
using UnityEngine;

/// <summary>
/// Added at runtime by CameraSetter to the virtual camera. Keeps the view inside a rectangle (a
/// CameraEvent's area) on top of whatever the scene's own Confiner 2D does, which is never touched.
///
/// Weight fades the push in and out (0 = off, 1 = fully inside), so entering and leaving an area
/// glides. When the area is smaller than the view the camera sits on the area's centre.
/// </summary>
[AddComponentMenu("")]
public class EventAreaConfiner : CinemachineExtension
{
    [HideInInspector] public Rect area;
    [HideInInspector] public float weight;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        // Body stage, like Confiner 2D. Added after it, so this runs second and sees its correction.
        if (stage != CinemachineCore.Stage.Body || weight <= 0f) return;

        Vector3 position = state.CorrectedPosition;

        float halfHeight = state.Lens.Orthographic
            ? state.Lens.OrthographicSize
            : Mathf.Abs(position.z) * Mathf.Tan(state.Lens.FieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * state.Lens.Aspect;

        Vector3 confined = position;
        confined.x = ClampAxis(position.x, area.xMin + halfWidth, area.xMax - halfWidth);
        confined.y = ClampAxis(position.y, area.yMin + halfHeight, area.yMax - halfHeight);

        state.PositionCorrection += (confined - position) * weight;
    }

    // Area narrower than the view on this axis: no room to move, so sit in the middle.
    private static float ClampAxis(float _value, float _min, float _max)
    {
        return _min > _max ? (_min + _max) * 0.5f : Mathf.Clamp(_value, _min, _max);
    }
}
