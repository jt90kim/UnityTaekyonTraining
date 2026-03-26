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

    private Dictionary<string, Transform> jointMap;

    void Awake()
    {
        jointMap = new Dictionary<string, Transform>();

        foreach (var jointName in SkeletonDefinition.JointNames)
        {
            Transform t = FindDeepChild(transform, jointName);

            if (t != null)
            {
                jointMap[jointName] = t;
            }
            else
            {
                Debug.LogError($"[SkeletonMapper] Missing joint in hierarchy: {jointName}");
            }
        }

        Debug.Log("[SkeletonMapper] Initialized joints: " + string.Join(", ", jointMap.Keys));
    }
    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            var result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
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
        //Debug.Log($"SPINE: {frame.joints["spine"]}");
        //Debug.Log($"NECK: {frame.joints["neck"]}");
        //Debug.Log($"HEAD: {frame.joints["head"]}");
        foreach (var kvp in frame.joints)
        {
            Debug.Log($"[DEBUG] Joint {kvp.Key} value = {kvp.Value}");

            var transform = GetJoint(kvp.Key);

            if (transform == null)
            {
                Debug.LogError("[DEBUG] Transform NULL for joint: " + kvp.Key);
                continue;
            }

            Debug.Log($"[DEBUG] BEFORE set {kvp.Key} local = {transform.localPosition}");

            Vector3 pos = kvp.Value;

            //// normalize around hip
            //if (kvp.Key == "hip")
            //{
            //    pos.x = 0;
            //    pos.z = 0;
            //}

            transform.localPosition = pos;

            Debug.Log($"[DEBUG] AFTER set {kvp.Key} local = {transform.localPosition}");
        }
    }
}
