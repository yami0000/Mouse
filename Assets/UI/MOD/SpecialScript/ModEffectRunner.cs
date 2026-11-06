using UnityEngine;

/// <summary>
/// Made on its own when the game starts (see ModEffects.CreateRunner) and kept across scenes.
/// Gives mod effects their per-frame OnWornUpdate, and holds what they create while worn.
/// </summary>
public class ModEffectRunner : MonoBehaviour
{
    private void Awake() => ModEffects.SetRoot(transform);

    // LateUpdate, so the weapon has already turned to the aim this frame.
    private void LateUpdate() => ModEffects.LateTick();

    private void OnDestroy() => ModEffects.StopAll();
}
