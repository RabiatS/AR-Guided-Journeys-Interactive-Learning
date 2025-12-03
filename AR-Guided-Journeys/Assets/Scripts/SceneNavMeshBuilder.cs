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
        Debug.Log("=== Scene loaded! Setting up obstacles ===");

        var room = MRUK.Instance.GetCurrentRoom();
        if (room != null)
        {
            int obstacleCount = 0;

            foreach (var anchor in room.Anchors)
            {
                // Only process furniture/walls, NOT floor or ceiling
                if (anchor.Label == MRUKAnchor.SceneLabels.FLOOR ||
                    anchor.Label == MRUKAnchor.SceneLabels.CEILING)
                {
                    continue;
                }

                // Add collider to make it an obstacle
                if (anchor.VolumeBounds.HasValue)
                {
                    var col = anchor.gameObject.GetComponent<BoxCollider>();
                    if (col == null)
                    {
                        col = anchor.gameObject.AddComponent<BoxCollider>();
                    }

                    // Make sure it's on a layer NavMesh can see
                    anchor.gameObject.layer = LayerMask.NameToLayer("Default");

                    obstacleCount++;
                    Debug.Log($"Added obstacle: {anchor.Label} at {anchor.transform.position}");
                }
            }

            Debug.Log($"=== Total obstacles added: {obstacleCount} ===");
        }

        Invoke("BakeNavMesh", 2f);
    }

    void BakeNavMesh()
    {
        if (navSurface != null)
        {
            // Make sure settings are correct
            navSurface.collectObjects = CollectObjects.All;
            navSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            navSurface.BuildNavMesh();
            Debug.Log("=== NavMesh baked! ===");
        }
    }
}
