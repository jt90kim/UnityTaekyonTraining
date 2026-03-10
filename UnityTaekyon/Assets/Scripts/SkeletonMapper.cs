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

        // Ensure all defined joints exist
        foreach (var name in Taekyon.SkeletonDefinition.JointNames)
        {
            if (!jointMap.ContainsKey(name))
            {
                Debug.LogWarning($"Missing joint binding: {name}");
            }
        }
    }

    public Transform GetJoint(string jointName)
    {
        Debug.Log("[JTK] SkeletonMapper with joints: " + string.Join(", ", jointMap.Keys));
        if (jointMap.TryGetValue(jointName, out var transform))
        {
            return transform;
        }

        Debug.LogWarning("Joint not found: " + jointName);
        return null;
    }

    public void ApplyFrame(MotionFrame frame)
    {
        if (frame?.joints == null)
        {
            Debug.LogError("[DEBUG] frame.joints is NULL");
            return;
        }

        foreach (var kvp in frame.joints)
        {
            Debug.Log($"[DEBUG] Joint {kvp.Key} value = {kvp.Value}");

            var transform = GetJoint(kvp.Key);

            if (transform == null)
            {
                Debug.LogError("[DEBUG] Transform NULL for joint: " + kvp.Key);
                continue;
            }

            Debug.Log($"[DEBUG] AFTER set {kvp.Key} local = {transform.localPosition}");

            transform.localPosition = kvp.Value;

            Debug.Log($"[DEBUG] AFTER set {kvp.Key} local = {transform.localPosition}");
        }
    }
}
