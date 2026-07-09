using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudienceController : MonoBehaviour
{
    [System.Serializable]
    public class AudienceGroup
    {
        public string groupName = "Normal";
        [Tooltip("All direct children of this Transform will bob. Assign a scene object, not a prefab asset.")]
        public Transform parent;
        [Tooltip("Vertical bob height in world units (min, max)")]
        public Vector2 amplitudeRange = new Vector2(0.03f, 0.08f);
        [Tooltip("Bob speed in radians per second (min, max). Use smaller values for aristocrats.")]
        public Vector2 frequencyRange = new Vector2(2f, 4f);
    }

    [Header("Audience Groups")]
    public List<AudienceGroup> groups = new List<AudienceGroup>();

    private struct Member
    {
        public Transform t;
        public float baseY;
        public float amplitude;
        public float frequency;
        public float phase;
    }

    private Member[] _members;

    private void Start()
    {
        List<Member> members = new List<Member>();

        foreach (AudienceGroup group in groups)
        {
            if (group.parent == null) continue;

            foreach (Transform child in group.parent)
            {
                Member m = new Member();
                m.t = child;
                m.baseY = child.localPosition.y;
                m.amplitude = Random.Range(group.amplitudeRange.x, group.amplitudeRange.y);
                m.frequency = Random.Range(group.frequencyRange.x, group.frequencyRange.y);
                m.phase = Random.Range(0f, Mathf.PI * 2f);
                members.Add(m);
            }
        }

        _members = members.ToArray();
    }

    private void Update()
    {
        float time = Time.time;

        for (int i = 0; i < _members.Length; i++)
        {
            Member m = _members[i];
            if (m.t == null) continue;

            Vector3 pos = m.t.localPosition;
            pos.y = m.baseY + Mathf.Sin(time * m.frequency + m.phase) * m.amplitude;
            m.t.localPosition = pos;
        }
    }
}