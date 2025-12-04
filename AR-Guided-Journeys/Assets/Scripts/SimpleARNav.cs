using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.AI;

public class SimpleARNav : MonoBehaviour
{
    public Transform target;
    public LineRenderer line;
    public float floorY = 0f;
    public float lineHeight = 0.15f;

    private NavMeshPath path;

    void Start()
    {
        path = new NavMeshPath();

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
        // Keep MRUK room locked to prevent drift
        // Smoother room stabilization

        // Stabilize ALL rooms so they all work together
        if (MRUK.Instance != null)
        {
            foreach (var room in MRUK.Instance.Rooms)
            {
                if (room != null && room.transform.position.magnitude > 0.01f)
                {
                    room.transform.position = Vector3.Lerp(room.transform.position, Vector3.zero, 0.1f);
                    room.transform.rotation = Quaternion.Lerp(room.transform.rotation, Quaternion.identity, 0.1f);
                }
            }
        }


        // Rest of navigation code
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
                corner.y = floorY + lineHeight;
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