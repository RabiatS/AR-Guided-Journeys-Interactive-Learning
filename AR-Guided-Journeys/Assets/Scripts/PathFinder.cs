using UnityEngine;
using System.Collections.Generic;

public class PathFinder : MonoBehaviour
{
    public Transform player;             // Camera/player/hmd transform
    public Waypoint destinationWaypoint; // Assign your target waypoint
    public List<Waypoint> waypoints;     // List of all waypoint objects
    public LineRenderer lineRenderer;
    public float lineWidth = 0.2f; 

    private Vector3 lastPlayerPos;
    private float minMoveDist = 0.2f; // Only update if moved > 20cm

    void Start()
    {
        lastPlayerPos = player.position;
        SetupLineRenderer();
        DrawGreedyPath();
    }

    void Update()
    {
        // Only redraw if player moved enough
        Vector3 flatPlayerPos = new Vector3(player.position.x, 0f, player.position.z);
        Vector3 flatLastPos = new Vector3(lastPlayerPos.x, 0f, lastPlayerPos.z);

        if (Vector3.Distance(flatPlayerPos, flatLastPos) > minMoveDist)
        {
            DrawGreedyPath();
            lastPlayerPos = player.position;
        }
    }

    void DrawGreedyPath()
    {
        Waypoint startWaypoint = FindClosestWaypoint(player.position);
        List<Waypoint> path = GreedyPath(startWaypoint, destinationWaypoint);

        List<Vector3> positions = new List<Vector3>();

        // Start slightly ahead of player at ground level
        Vector3 groundStart = player.position;
        groundStart.y = 0f;
        groundStart += player.forward * 0.5f; // Offset forward for clarity
        positions.Add(groundStart);

        foreach (Waypoint wp in path)
        {
            Vector3 wpPos = wp.transform.position;
            wpPos.y = 0f; // Flatten to ground
            positions.Add(wpPos);
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

    Waypoint FindClosestWaypoint(Vector3 position)
    {
        float minDist = float.MaxValue;
        Waypoint closest = null;
        foreach (Waypoint wp in waypoints)
        {
            float dist = Vector3.Distance(wp.transform.position, position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = wp;
            }
        }
        return closest;
    }

    List<Waypoint> GreedyPath(Waypoint start, Waypoint destination)
    {
        List<Waypoint> path = new List<Waypoint>();
        HashSet<Waypoint> visited = new HashSet<Waypoint>();

        Waypoint current = start;
        path.Add(current);
        visited.Add(current);

        int maxSteps = 100;

        while (current != destination && maxSteps > 0)
        {
            Waypoint next = null;
            float minDist = float.MaxValue;

            foreach (Waypoint neighbor in current.neighbors)
            {
                if (visited.Contains(neighbor)) continue;
                float dist = Vector3.Distance(neighbor.transform.position, destination.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    next = neighbor;
                }
            }

            if (next == null)
                break;

            path.Add(next);
            visited.Add(next);
            current = next;
            maxSteps--;
        }
        return path;
    }

    void SetupLineRenderer()
    {
        // Set line appearance as needed
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.widthMultiplier = lineWidth;
        


}
}
