using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public List<Transform> waypoints; // Assign in inspector!
    public LineRenderer lineRenderer; // Assign in inspector!

    public void FindRoute()
    {
        // Simple "closest connection" approach (for grid: use A*)
        List<Vector3> path = new List<Vector3>();
        path.Add(startPoint.position);

        Transform current = startPoint;
        while (current != endPoint)
        {
            Transform next = null;
            float minDist = float.MaxValue;

            foreach (Transform wp in waypoints)
            {
                if (wp == current) continue;
                float d = Vector3.Distance(current.position, wp.position) + Vector3.Distance(wp.position, endPoint.position);
                if (d < minDist)
                {
                    minDist = d;
                    next = wp;
                }
            }
            if (next == null) break;

            path.Add(next.position);
            current = next;
        }
        path.Add(endPoint.position);

        // Draw the line
        lineRenderer.positionCount = path.Count;
        lineRenderer.SetPositions(path.ToArray());
        lineRenderer.material.color = Color.red;
        lineRenderer.widthMultiplier = 0.5f;
    }
}
