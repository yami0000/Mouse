using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetter : MonoBehaviour
{
    public static CameraSetter Instance { get; private set; }


    [SerializeField] private CinemachineVirtualCamera _vcam;

    private Vector3 current;
    Transform player;

    // Event areas are applied on top of the scene's own Confiner 2D, which is never touched.
    private EventAreaConfiner areaConfiner;
    private Coroutine blendRoutine;

    private void Awake()
    {
        // Setup the Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        player = PlayerManager.Instance.player.transform;
       current = _vcam.transform.position;
        _vcam.Follow = player;
    }


    public void EnablePosition(Transform T)
    {
        _vcam.Follow = T;
    }

    public void DisablePosition()
    {
        _vcam.Follow = player;
    }

    #region Confiner

    /// <summary>
    /// Keeps the camera inside _area while it goes on following the player; the scene's own bounds still
    /// apply too. When _area is smaller than the view the camera sits on its centre. _blend = seconds to ease in.
    /// </summary>
    public void ConfineTo(Bounds _area, float _blend = 0f)
    {
        // Added after the scene's Confiner 2D, so it runs after it and only ever narrows things further.
        if (areaConfiner == null)
            areaConfiner = _vcam.gameObject.AddComponent<EventAreaConfiner>();

        areaConfiner.area = Rect.MinMaxRect(_area.min.x, _area.min.y, _area.max.x, _area.max.y);
        BlendWeight(1f, _blend);
    }

    /// <summary>Lets the camera out of the event area again. _blend = seconds to ease out.</summary>
    public void ResetConfiner(float _blend = 0f)
    {
        if (areaConfiner != null)
            BlendWeight(0f, _blend);
    }

    private void BlendWeight(float _to, float _time)
    {
        if (blendRoutine != null)
            StopCoroutine(blendRoutine);

        blendRoutine = StartCoroutine(Blend(_to, _time));
    }

    // Fades how hard the area pushes the camera, so it glides in and out instead of snapping.
    private IEnumerator Blend(float _to, float _time)
    {
        float from = areaConfiner.weight;

        for (float t = 0f; t < _time; t += Time.deltaTime)
        {
            areaConfiner.weight = Mathf.Lerp(from, _to, Mathf.SmoothStep(0f, 1f, t / _time));
            yield return null;
        }

        areaConfiner.weight = _to;
        blendRoutine = null;
    }

    #endregion
}
