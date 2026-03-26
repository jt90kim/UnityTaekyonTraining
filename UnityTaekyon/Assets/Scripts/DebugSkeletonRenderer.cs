using System.Collections.Generic;
using UnityEngine;
using Taekyon;

public class DebugSkeletonRenderer : MonoBehaviour
{
    [Header("Assign ALL joints here")]
    public List<Transform> joints = new List<Transform>();

    public float jointSize = 0.06f;
    public float boneWidth = 0.12f;

    private Dictionary<string, Transform> jointMap = new Dictionary<string, Transform>();

    // FIX: map joint name → sphere
    private Dictionary<string, GameObject> jointSpheres = new Dictionary<string, GameObject>();

    private struct BoneRender
    {
        public LineRenderer line;
        public Transform a;
        public Transform b;
    }

    private List<BoneRender> boneLines = new List<BoneRender>();


    void Start()
    {
        BuildJointMap();
        CreateJointSpheres();
        CreateBoneLines();
    }


    void Update()
    {
        UpdateJointSpheres();
        UpdateBoneLines();
    }


    void BuildJointMap()
    {
        jointMap.Clear();

        foreach (var j in joints)
        {
            if (j == null) continue;

            if (!jointMap.ContainsKey(j.name))
                jointMap.Add(j.name, j);
        }

        Debug.Log("[Renderer] Joints: " + string.Join(", ", jointMap.Keys));
    }


    void CreateJointSpheres()
    {
        foreach (var kvp in jointMap)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            sphere.name = "joint_" + kvp.Key;
            sphere.transform.parent = transform;
            sphere.transform.localScale = Vector3.one * jointSize;

            var renderer = sphere.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = Color.yellow;

            jointSpheres[kvp.Key] = sphere;

            // Optional: trail for debugging motion
            if (kvp.Key == "ankle_r")
            {
                var trail = sphere.AddComponent<TrailRenderer>();
                trail.time = 0.5f;
                trail.startWidth = 0.1f;
                trail.endWidth = 0.02f;
                trail.material = new Material(Shader.Find("Sprites/Default"));
                trail.startColor = Color.red;
                trail.endColor = Color.yellow;
            }
        }
    }


    void CreateBoneLines()
    {
        boneLines.Clear();

        foreach (var bone in SkeletonDefinition.Bones)
        {
            if (!jointMap.ContainsKey(bone.Item1) || !jointMap.ContainsKey(bone.Item2))
            {
                Debug.LogWarning($"[Renderer] Missing joint for bone {bone.Item1} → {bone.Item2}");
                continue;
            }

            Transform a = jointMap[bone.Item1];
            Transform b = jointMap[bone.Item2];

            GameObject go = new GameObject($"bone_{bone.Item1}_{bone.Item2}");
            go.transform.parent = transform;

            LineRenderer lr = go.AddComponent<LineRenderer>();

            lr.positionCount = 2;
            lr.useWorldSpace = true;

            // FIX: width consistency
            lr.startWidth = boneWidth;
            lr.endWidth = boneWidth;

            lr.material = new Material(Shader.Find("Sprites/Default"));
            Color c = GetBoneColor(bone.Item1, bone.Item2);
            lr.startColor = c;
            lr.endColor = c;

            BoneRender br = new BoneRender
            {
                line = lr,
                a = a,
                b = b
            };

            boneLines.Add(br);
        }
    }

    Color GetBoneColor(string a, string b)
    {
        // legs
        if (a.Contains("knee") || a.Contains("ankle"))
            return Color.green;

        // arms
        if (a.Contains("shoulder") || a.Contains("elbow") || a.Contains("wrist"))
            return Color.cyan;

        // torso
        if (a == "hip" || a == "spine" || a == "neck" || a == "head")
            return Color.yellow;

        return Color.white;
    }


    void UpdateJointSpheres()
    {
        foreach (var kvp in jointMap)
        {
            if (!jointSpheres.ContainsKey(kvp.Key)) continue;

            jointSpheres[kvp.Key].transform.position = kvp.Value.position;
        }
    }


    void UpdateBoneLines()
    {
        foreach (var bone in boneLines)
        {
            if (bone.a == null || bone.b == null) continue;

            bone.line.SetPosition(0, bone.a.position);
            bone.line.SetPosition(1, bone.b.position);
        }
    }
}