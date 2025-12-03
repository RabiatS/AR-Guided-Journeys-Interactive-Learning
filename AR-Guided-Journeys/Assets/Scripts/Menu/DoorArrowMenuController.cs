using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction.Samples;

/// <summary>
/// Connects a dropdown style menu to the door direction arrows in the scene.
/// Assign the arrows in the inspector (index must match the dropdown option order).
/// </summary>
public class DoorArrowMenuController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Dropdown component driving the selection (uGUI implementation). Optional.")]
    [SerializeField] Dropdown uiDropdown;

    [Tooltip("Meta Interaction SDK DropDownGroup.")]
    [SerializeField] CustomDropDownGroup dropDownGroup;

    [Header("Door Direction Arrows")]
    [Tooltip("Arrow objects to toggle. List index must match the dropdown option index.")]
    [SerializeField] List<GameObject> doorArrows = new List<GameObject>();

    [Tooltip("Hide every arrow automatically when this component is enabled.")]
    [SerializeField] bool hideAllOnEnable = true;

    [Tooltip("Hide all arrows if the dropdown value is outside the configured list range.")]
    [SerializeField] bool hideOnInvalidSelection = true;

    void Reset()
    {
        AutoAssignDropdowns();
    }

    void Awake()
    {
        AutoAssignDropdowns();
    }

    void OnEnable()
    {
        RegisterDropdownCallbacks();

        if (hideAllOnEnable)
        {
            HideAllArrows();
        }

        SyncToCurrentSelection();
    }

    void OnDisable()
    {
        UnregisterDropdownCallbacks();
    }

    /// <summary>
    /// Public entry point that can be wired directly to dropdown events via inspector.
    /// </summary>
    /// <param name="optionIndex">Selected dropdown option.</param>
    public void HandleSelectionChanged(int optionIndex)
    {
        if (IsValidIndex(optionIndex))
        {
            ShowArrowAt(optionIndex);
        }
        else if (hideOnInvalidSelection)
        {
            HideAllArrows();
        }
    }

    void SyncToCurrentSelection()
    {
        int selection = -1;

        if (dropDownGroup)
        {
            selection = dropDownGroup.SelectedIndex;
        }
        else if (uiDropdown)
        {
            selection = uiDropdown.value;
        }

        if (selection >= 0)
        {
            HandleSelectionChanged(selection);
        }
        else if (hideOnInvalidSelection)
        {
            HideAllArrows();
        }
    }

    void RegisterDropdownCallbacks()
    {
        if (uiDropdown)
        {
            uiDropdown.onValueChanged.AddListener(HandleSelectionChanged);
        }

        if (dropDownGroup)
        {
            dropDownGroup.WhenSelectionChanged.AddListener(HandleSelectionChanged);
        }
    }

    void UnregisterDropdownCallbacks()
    {
        if (uiDropdown)
        {
            uiDropdown.onValueChanged.RemoveListener(HandleSelectionChanged);
        }

        if (dropDownGroup)
        {
            dropDownGroup.WhenSelectionChanged.RemoveListener(HandleSelectionChanged);
        }
    }

    void AutoAssignDropdowns()
    {
        if (!uiDropdown)
        {
            uiDropdown = GetComponent<Dropdown>();
        }

        if (!dropDownGroup)
        {
            dropDownGroup = GetComponent<CustomDropDownGroup>();
        }
    }

    void ShowArrowAt(int index)
    {
        for (int i = 0; i < doorArrows.Count; i++)
        {
            if (!doorArrows[i])
            {
                continue;
            }

            bool shouldBeVisible = i == index;
            if (doorArrows[i].activeSelf != shouldBeVisible)
            {
                doorArrows[i].SetActive(shouldBeVisible);
            }
        }
    }

    void HideAllArrows()
    {
        for (int i = 0; i < doorArrows.Count; i++)
        {
            if (doorArrows[i] && doorArrows[i].activeSelf)
            {
                doorArrows[i].SetActive(false);
            }
        }
    }

    bool IsValidIndex(int index)
    {
        return index >= 0 && index < doorArrows.Count;
    }
}
