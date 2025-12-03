using UnityEngine;
using Oculus.Interaction.Samples;

/// <summary>
/// Bridges the Meta dropdown (CustomDropDownGroup) and the navigation guider.
/// - Listens to dropdown selection index.
/// - Maps each index to a door waypoint.
/// - Notifies GuiderController of the chosen destination.
/// </summary>
public class DestinationBinder : MonoBehaviour
{
    [Header("Menu & Guider References")]
    [Tooltip("Dropdown group that represents the door selection menu.")]
    public CustomDropDownGroup dropDownGroup;

    [Tooltip("Navigation guider robot controller.")]
    public GuiderController guider;

    [Header("Door Waypoints (order must match menu options)")]
    [Tooltip("Waypoints for each door option. Index 0 -> Option 1, etc.")]
    public Transform[] doorWaypoints;

    void Awake()
    {
        if (!dropDownGroup)
        {
            dropDownGroup = GetComponent<CustomDropDownGroup>();
        }

        if (!guider)
        {
            //guider = FindObjectOfType<GuiderController>();
        }
    }

    void OnEnable()
    {
        if (dropDownGroup != null)
        {
            dropDownGroup.WhenSelectionChanged.AddListener(OnSelectionChanged);
        }
    }

    void OnDisable()
    {
        if (dropDownGroup != null)
        {
            dropDownGroup.WhenSelectionChanged.RemoveListener(OnSelectionChanged);
        }
    }

    void OnSelectionChanged(int index)
    {
        if (guider == null)
        {
            return;
        }

        if (index < 0 || index >= doorWaypoints.Length)
        {
            return;
        }

        Transform waypoint = doorWaypoints[index];
        if (waypoint == null)
        {
            return;
        }

        guider.SetDestination(waypoint.position);
    }
}
