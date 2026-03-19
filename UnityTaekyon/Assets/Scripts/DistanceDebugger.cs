using UnityEngine;

public class DistanceDebugger : MonoBehaviour
{
    public Transform opponentHip;
    public Transform playerHip;

    void Update()
    {
        if (opponentHip == null || playerHip == null)
            return;

        float d = Vector3.Distance(opponentHip.position, playerHip.position);

        Debug.Log("[DISTANCE] " + d.ToString("F2"));
    }
}