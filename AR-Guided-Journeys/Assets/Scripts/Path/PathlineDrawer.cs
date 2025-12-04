using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathlineDrawer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The arrow prefab to instantiate along the path")]
    public GameObject arrowPrefab;

    [Tooltip("Distance between arrows")]
    public float arrowSpacing = 0.5f;

    [Tooltip("Height offset from the ground")]
    public float heightOffset = 0.1f;

    [Tooltip("The user's transform (start of the path)")]
    public Transform userTransform;

    private List<GameObject> spawnedArrows = new List<GameObject>();
    private NavMeshPath navMeshPath;

    void Awake()
    {
        navMeshPath = new NavMeshPath();
    }

    public void DrawPathTo(Vector3 targetPosition)
    {
        if (userTransform == null)
        {
            Debug.LogWarning("PathlineDrawer: User Transform is not assigned.");
            return;
        }

        // Calculate path on NavMesh
        if (NavMesh.CalculatePath(userTransform.position, targetPosition, NavMesh.AllAreas, navMeshPath))
        {
            if (navMeshPath.status == NavMeshPathStatus.PathComplete || navMeshPath.status == NavMeshPathStatus.PathPartial)
            {
                GenerateArrows(navMeshPath.corners);
            }
            else
            {
                Debug.LogWarning("PathlineDrawer: Path not found or invalid.");
                ClearPath();
            }
        }
        else
        {
            Debug.LogWarning("PathlineDrawer: Failed to calculate path.");
            ClearPath();
        }
    }

    private void GenerateArrows(Vector3[] corners)
    {
        ClearPath();

        if (corners.Length < 2) return;

        if (arrowPrefab == null)
        {
            Debug.LogWarning("PathlineDrawer: Arrow Prefab is not assigned.");
            return;
        }

        // Iterate through path segments
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[i + 1];
            float segmentLength = Vector3.Distance(start, end);
            Vector3 direction = (end - start).normalized;

            // Place arrows along the segment
            float distanceCovered = 0f;
            while (distanceCovered < segmentLength)
            {
                Vector3 position = start + direction * distanceCovered;
                
                // Adjust height
                position.y += heightOffset;

                GameObject arrow = Instantiate(arrowPrefab, position, Quaternion.LookRotation(direction));
                spawnedArrows.Add(arrow);

                distanceCovered += arrowSpacing;
            }
        }
        
        // Ensure the last point also has an arrow or at least we reach near it
        // (Optional: depending on visual preference)
    }

    public void ClearPath()
    {
        foreach (var arrow in spawnedArrows)
        {
            if (arrow != null) Destroy(arrow);
        }
        spawnedArrows.Clear();
    }
}
