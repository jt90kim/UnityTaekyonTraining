using UnityEngine;
using System.Collections.Generic;

namespace Taekyon
{
    public class HumanoidBodyRenderer : MonoBehaviour
    {
        public Material bodyMaterial;

        private List<(Transform a, Transform b, Transform capsule)> limbCapsules
            = new List<(Transform, Transform, Transform)>();

        private Transform hip;
        private Transform kneeL;
        private Transform kneeR;

        void Start()
        {
            hip = transform.Find("hip");
            Debug.Log("Hip found: " + (hip != null));

            if (hip != null)
            {
                kneeL = hip.Find("knee_l");
                kneeR = hip.Find("knee_r");

                Debug.Log("[HumanoidBodyRenderer] knee_l found: " + (kneeL != null));
                Debug.Log("[HumanoidBodyRenderer] knee_r found: " + (kneeR != null));
            }

            if (hip != null && kneeL != null)
            {
                CreateLimb(hip, kneeL);
            }

            //if (hip != null && kneeR != null)
            //    CreateLimb(hip, kneeR);

            //if (hip != null) { }
            //    CreateTorso();
        }

        void CreateLimb(Transform a, Transform b)
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);

            capsule.transform.SetParent(null);
            capsule.transform.localScale = Vector3.one;

            capsule.GetComponent<Collider>().enabled = false;

            if (bodyMaterial != null)
                capsule.GetComponent<MeshRenderer>().material = bodyMaterial;

            limbCapsules.Add((a, b, capsule.transform));
        }

        void CreateTorso()
        {
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            torso.transform.SetParent(hip);

            torso.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            torso.transform.localPosition = new Vector3(0, 0.6f, 0);

            torso.GetComponent<Collider>().enabled = false;

            if (bodyMaterial != null)
                torso.GetComponent<MeshRenderer>().material = bodyMaterial;
        }

        void Update()
        {
           
                if (hip != null)
                {
                    Debug.Log("[HumanoidBodyRenderer] HIP LOCAL: " + hip.localPosition);
                Debug.Log("[HumanoidBodyRenderer] HIP WORLD POSITION: " + hip.position);
            }
 
            foreach (var limb in limbCapsules)
            {
                Transform a = limb.a;
                Transform b = limb.b;
                Transform capsule = limb.capsule;

                Vector3 midPoint = (a.position + b.position) / 2f;
                capsule.position = midPoint;

                Vector3 direction = b.position - a.position;
                capsule.up = direction.normalized;

                float length = direction.magnitude;
                //capsule.localScale = new Vector3(0.2f, length / 2f, 0.2f);
                capsule.localScale = new Vector3(0.15f, length * 0.5f, 0.15f);
            }
        }
    }
}