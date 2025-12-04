using Meta.XR.MRUtilityKit;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class SceneNavMeshBuilder : MonoBehaviour
{
    public NavMeshSurface navSurface;

    void Start()
    {
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        }
    }

    void OnSceneLoaded()
    {
        Debug.Log("=== Scene loaded! Processing active rooms only ===");

        foreach (var room in MRUK.Instance.Rooms)
        {
            // Skip inactive rooms
            if (room == null || !room.gameObject.activeInHierarchy) continue;

            Debug.Log($"Processing active room: {room.name}");

            // Rest of your existing code for floor and obstacles...
            foreach (var anchor in room.Anchors)
            {
                // Your existing floor and obstacle code stays the same
                if (anchor.Label == MRUKAnchor.SceneLabels.FLOOR)
                {
                    MeshCollider floorCol = anchor.gameObject.GetComponent<MeshCollider>();
                    if (floorCol == null)
                    {
                        floorCol = anchor.gameObject.AddComponent<MeshCollider>();
                    }
                    floorCol.convex = false;
                    continue;
                }

                if (anchor.Label == MRUKAnchor.SceneLabels.CEILING) continue;

                if (anchor.VolumeBounds.HasValue)
                {
                    foreach (var col in anchor.GetComponents<Collider>())
                    {
                        Destroy(col);
                    }

                    BoxCollider boxCol = anchor.gameObject.AddComponent<BoxCollider>();
                    Vector3 size = anchor.VolumeBounds.Value.size;
                    size.y = 2.5f;
                    boxCol.size = size;

                    Vector3 center = anchor.VolumeBounds.Value.center;
                    center.y = 1.25f;
                    boxCol.center = center;

                    anchor.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }

        Invoke("BakeNavMesh", 2f);
    }




    void BakeNavMesh()
    {
        if (navSurface != null)
        {
            navSurface.collectObjects = CollectObjects.All;
            navSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            navSurface.BuildNavMesh();
            Debug.Log("=== NavMesh BAKED! ===");
        }
    }
}
