using System.Collections.Generic;
using UnityEngine;
using Taekyon;

public class SkeletonMapper : MonoBehaviour
{
    [System.Serializable]
    public class JointBinding
    {
        public string jointName;
        public Transform jointTransform;
    }

    public List<JointBinding> joints = new List<JointBinding>();

    private Dictionary<string, Transform> jointMap;

    void Awake()
    {
        jointMap = new Dictionary<string, Transform>();

        foreach (var joint in joints)
        {
            if (!jointMap.ContainsKey(joint.jointName))
            {
                jointMap.Add(joint.jointName, joint.jointTransform);
            }
        }

       // Debug.Log("SkeletonMapper initialized with joints: " + string.Join(", ", jointMap.Keys));
    }

    public Transform GetJoint(string jointName)
    {
        if (jointMap.TryGetValue(jointName, out var transform))
        {
            return transform;
        }

        Debug.LogWarning("Joint not found: " + jointName);
        return null;
    }

    public void ApplyFrame(MotionFrame frame)
    {
        if (frame?.joints == null) return;
        foreach (var kvp in frame.joints)
        {
            var transform = GetJoint(kvp.Key);
            if (transform != null)
            {
                transform.localPosition = kvp.Value;
               // Debug.Log($"Applied joint {kvp.Key}: {kvp.Value}");
            }
        }
    }
}
