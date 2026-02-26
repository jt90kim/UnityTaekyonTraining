using UnityEngine;


namespace Taekyon
{
    public class DebugJointVisualizer : MonoBehaviour
    {
        public float jointScale = 0.2f;

        void Start()
        {
            foreach (Transform joint in GetComponentsInChildren<Transform>())
            {
                if (joint == transform)
                    continue;

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(joint);
                sphere.transform.localPosition = Vector3.zero;
                sphere.transform.localRotation = Quaternion.identity;
                sphere.transform.localScale = Vector3.one * jointScale;

                Destroy(sphere.GetComponent<Collider>());
            }
        }
    }
}