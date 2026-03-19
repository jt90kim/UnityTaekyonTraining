using System.Collections.Generic;
using UnityEngine;
using Taekyon;

public class DebugSkeletonRenderer : MonoBehaviour
{
    public List<Transform> joints = new List<Transform>();

    public float jointSize = 0.06f;
    public float boneWidth = 0.12f;

    private Dictionary<string, Transform> jointMap = new Dictionary<string, Transform>();

    private List<GameObject> jointSpheres = new List<GameObject>();

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

            jointSpheres.Add(sphere);

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
        foreach (var bone in SkeletonDefinition.Bones)
        {
            if (!jointMap.ContainsKey(bone.Item1) || !jointMap.ContainsKey(bone.Item2))
            {
                Debug.LogWarning($"Missing joint for bone {bone.Item1} → {bone.Item2}");
                continue;
            }

            Transform a = jointMap[bone.Item1];
            Transform b = jointMap[bone.Item2];

            GameObject go = new GameObject($"bone_{bone.Item1}_{bone.Item2}");
            go.transform.parent = transform;

            LineRenderer lr = go.AddComponent<LineRenderer>();

            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.widthMultiplier = boneWidth;

            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;

            BoneRender br = new BoneRender
            {
                line = lr,
                a = a,
                b = b
            };

            boneLines.Add(br);
        }
    }


    void UpdateJointSpheres()
    {
        int i = 0;

        foreach (var kvp in jointMap)
        {
            jointSpheres[i].transform.position = kvp.Value.position;
            i++;
        }
    }


    void UpdateBoneLines()
    {
        foreach (var bone in boneLines)
        {
            bone.line.SetPosition(0, bone.a.position);
            bone.line.SetPosition(1, bone.b.position);
        }
    }
}