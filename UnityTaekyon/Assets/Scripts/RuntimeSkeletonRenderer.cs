using UnityEngine;

namespace Taekyon
{
    public class RuntimeSkeletonRenderer : MonoBehaviour
    {
        public Transform hip;
        public Transform knee_l;
        public Transform knee_r;

        private LineRenderer lineHipKneeL;
        private LineRenderer lineHipKneeR;

        void Start()
        {
            lineHipKneeL = CreateLine("hip_knee_l");
            lineHipKneeR = CreateLine("hip_knee_r");

            // Ensure skeleton is visible even before motion playback
            if (hip != null && knee_l != null && knee_r != null)
            {
                if (hip.localPosition == Vector3.zero)
                    hip.localPosition = new Vector3(0, 1, 2);

                if (knee_l.localPosition == Vector3.zero)
                    knee_l.localPosition = new Vector3(-0.3f, 0.5f, 2);

                if (knee_r.localPosition == Vector3.zero)
                    knee_r.localPosition = new Vector3(0.3f, 0.5f, 2);
            }

            UpdateLines();
        }

        void Update()
        {
            UpdateLines();
        }

        LineRenderer CreateLine(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = transform;

            LineRenderer lr = go.AddComponent<LineRenderer>();

            lr.positionCount = 2;
            lr.useWorldSpace = true;

            lr.material = new Material(Shader.Find("Sprites/Default"));

            lr.widthMultiplier = 0.15f;

            lr.startColor = Color.white;
            lr.endColor = Color.white;

            return lr;
        }

        void UpdateLines()
        {
            if (hip == null || knee_l == null || knee_r == null)
                return;

            lineHipKneeL.SetPosition(0, hip.position);
            lineHipKneeL.SetPosition(1, knee_l.position);

            lineHipKneeR.SetPosition(0, hip.position);
            lineHipKneeR.SetPosition(1, knee_r.position);
        }
    }
}