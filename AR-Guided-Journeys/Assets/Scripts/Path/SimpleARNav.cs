using UnityEngine;
using UnityEngine.AI;

public class SimpleARNav : MonoBehaviour
{
    public Transform target;
    public LineRenderer line;
    public float floorY = 0f;
    public float lineHeight = 0.05f; // Raise slightly above floor

    private NavMeshPath path;

    void Start()
    {
        path = new NavMeshPath();

        // Configure LineRenderer for VR visibility
        if (line != null)
        {
            line.useWorldSpace = true;
            line.startWidth = 0.15f;
            line.endWidth = 0.15f;
            line.numCornerVertices = 5;
            line.numCapVertices = 5;
        }
    }

    void Update()
    {
        if (!line || !target || !Camera.main) return;

        var camPos = Camera.main.transform.position;
        Vector3 start = new Vector3(camPos.x, floorY + lineHeight, camPos.z);
        Vector3 end = new Vector3(target.position.x, floorY + lineHeight, target.position.z);

        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path) && path.corners.Length > 0)
        {
            line.positionCount = path.corners.Length;
            for (int i = 0; i < path.corners.Length; i++)
            {
                Vector3 corner = path.corners[i];
                corner.y = floorY + lineHeight; // Keep all points at same height
                line.SetPosition(i, corner);
            }
        }
        else
        {
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }
    }
}
