using UnityEngine;
using System.Collections.Generic;

namespace Taekyon
{
    public class RuntimeBoneRenderer : MonoBehaviour
    {
        private List<(Transform joint, Transform parent, LineRenderer line)> bones
            = new List<(Transform, Transform, LineRenderer)>();

        public float jointScale = 0.2f;

        void Start()
        {
            foreach (Transform joint in GetComponentsInChildren<Transform>())
            {
                if (joint == transform)
                    continue;

                // Create joint sphere
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(joint);
                sphere.transform.localPosition = Vector3.zero;
                sphere.transform.localScale = Vector3.one * jointScale;
                Destroy(sphere.GetComponent<Collider>());

                // Create bone line
                if (joint.parent != null && joint.parent != transform)
                {
                    GameObject lineObj = new GameObject("BoneLine");
                    lineObj.transform.SetParent(transform);

                    var line = lineObj.AddComponent<LineRenderer>();
                    line.positionCount = 2;
                    line.startWidth = 0.03f;
                    line.endWidth = 0.03f;
                    line.material = new Material(Shader.Find("Sprites/Default"));

                    bones.Add((joint, joint.parent, line));
                }
            }
        }

        void Update()
        {
            foreach (var bone in bones)
            {
                bone.line.SetPosition(0, bone.joint.position);
                bone.line.SetPosition(1, bone.parent.position);
            }
        }
    }
}