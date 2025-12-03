using UnityEngine;
using UnityEngine.AI;

public class IndoorNavigation : MonoBehaviour
{
    public Transform navigationTarget;
    public LineRenderer lineRenderer;
    public Transform player;

    private NavMeshPath navMeshPath;

    void Start()
    {
        navMeshPath = new NavMeshPath();

        if (player == null)
        {
            // Find CenterEyeAnchor if using OVRCameraRig
            GameObject centerEye = GameObject.Find("CenterEyeAnchor");
            if (centerEye != null)
                player = centerEye.transform;
            else
                player = Camera.main.transform;
        }

        Debug.Log("IndoorNavigation Started");
        Debug.Log($"Player: {player?.name}, Target: {navigationTarget?.name}, LineRenderer: {lineRenderer != null}");
    }

    void Update()
    {
        CalculatePath();
    }

    void CalculatePath()
    {
        if (navigationTarget == null || lineRenderer == null || player == null)
        {
            Debug.LogError("Missing references!");
            return;
        }

        NavMesh.CalculatePath(
            player.position,
            navigationTarget.position,
            NavMesh.AllAreas,
            navMeshPath
        );

        Debug.Log($"Path status: {navMeshPath.status}, Corners: {navMeshPath.corners.Length}");

        if (navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            lineRenderer.positionCount = navMeshPath.corners.Length;
            lineRenderer.SetPositions(navMeshPath.corners);
            Debug.Log($"Line rendered with {navMeshPath.corners.Length} points");
        }
        else
        {
            Debug.LogWarning($"Path failed: {navMeshPath.status}");
        }
    }
}
