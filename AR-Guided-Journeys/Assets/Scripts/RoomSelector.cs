using UnityEngine;
using Meta.XR.MRUtilityKit;

public class RoomSelector : MonoBehaviour
{
    void Start()
    {
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        }
    }

    void OnSceneLoaded()
    {
        Debug.Log("=== Activating ALL rooms ===");

        // Show every room
        foreach (var room in MRUK.Instance.Rooms)
        {
            if (room != null)
            {
                room.gameObject.SetActive(true);
                Debug.Log($"Activated room: {room.name}");
            }
        }
    }
}
