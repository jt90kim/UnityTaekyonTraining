using UnityEngine;

namespace Taekyon
{
    public class DebugBoneDrawer : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            foreach (Transform joint in GetComponentsInChildren<Transform>())
            {
                if (joint.parent != null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(joint.position, joint.parent.position);
                }
            }
        }
    }
}