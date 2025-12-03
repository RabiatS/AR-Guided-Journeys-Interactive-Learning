using UnityEngine;
using Meta.XR.MRUtilityKit;

public class SceneVisibilityToggle : MonoBehaviour
{
    public OVRInput.Button toggleButton = OVRInput.Button.Two; // B button
    private bool wireframeVisible = true;

    void Update()
    {
        if (OVRInput.GetDown(toggleButton))
        {
            Debug.Log("B button pressed!");
            wireframeVisible = !wireframeVisible;
            ToggleSceneVisibility(wireframeVisible);
        }
    }

    void ToggleSceneVisibility(bool visible)
    {
        if (MRUK.Instance == null)
        {
            Debug.LogError("MRUK Instance is null!");
            return;
        }

        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogError("No room loaded!");
            return;
        }

        int toggledCount = 0;
        foreach (var anchor in room.Anchors)
        {
            var renderer = anchor.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = visible;
                toggledCount++;
            }
        }

        Debug.Log($"Wireframe: {(visible ? "ON" : "OFF")} - Toggled {toggledCount} objects");
    }
}
