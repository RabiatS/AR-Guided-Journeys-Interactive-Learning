using UnityEngine;
using Meta.XR.MRUtilityKit;

public class SceneMeshPinPlacer : MonoBehaviour
{
    [Header("Input & Prefab")]
    [Tooltip("Controller transform used as ray origin & direction")]
    public Transform controller;

    [Tooltip("Destination pin prefab to instantiate on the floor")]
    public GameObject pinPrefab;

    [Header("MRUK Label Filter")]
    [Tooltip("Which MRUK scene labels the raycast is allowed to hit (e.g. FLOOR, TABLE, WALL_FACE). Multiple flags can be combined.")]
    public MRUKAnchor.SceneLabels labelFilter = MRUKAnchor.SceneLabels.FLOOR;

    [Header("Raycast Settings")]
    public float rayDistance = 10f;

    [Header("Path Drawing")]
    public PathlineDrawer pathDrawer;

    [Header("Robot Guider")]
    [Tooltip("Optional binder that will send the pin position to the robot guider.")]
    public PinDestinationBinder pinDestinationBinder;

    // 如果只允许场景中存在一个 pin，可以缓存它
    private GameObject currentPinInstance;

    void Update()
    {
        // 这里以 Meta / Oculus 的右手扳机为例，你可以根据具体输入系统调整
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            TryPlacePin();
        }
    }

    private void TryPlacePin()
    {
        if (controller == null || pinPrefab == null || MRUK.Instance == null)
        {
            Debug.LogWarning("PinPlacer: controller/pinPrefab/MRUK.Instance is null");

            return;
        }

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null)
        {
            Debug.LogWarning("PinPlacer: current room is null");
            return;
        }

        Ray ray = new Ray(controller.position, controller.forward);

        // 使用 MRUKRoom 的 Raycast 命中当前房间的场景网格（EffectMesh）
        RaycastHit hit;
        MRUKAnchor anchor; // 这里我们只需要 hit.point / hit.normal，anchor 备用

        // 使用在 Inspector 中设置的 labelFilter（可多选标记）
        bool hitSomething = room.Raycast(
            ray,
            rayDistance,
            LabelFilter.FromEnum(labelFilter),
            out hit,
            out anchor
        );
        
        Debug.Log($"PinPlacer: hitSomething={hitSomething}, labelFilter={labelFilter}");

        if (hitSomething)
        {
            Vector3 hitPoint = hit.point;
            Vector3 hitNormal = hit.normal;
            Vector3 tangentForward = controller.forward;

            // Project controller forward onto the hit surface so the pin keeps standing up.
            Vector3 projectedForward = Vector3.ProjectOnPlane(tangentForward, hitNormal);
            if (projectedForward.sqrMagnitude < 0.001f)
            {
                projectedForward = Vector3.forward;
            }

            Quaternion pinRotation = Quaternion.LookRotation(projectedForward.normalized, hitNormal);

            // 如果只要一个 pin，则移动/重用
            if (currentPinInstance == null)
            {
                currentPinInstance = Instantiate(
                    pinPrefab,
                    hitPoint,
                    pinRotation
                );
                // 确保生成的 pin 是显示的（即使 prefab 本身被隐藏了）
                currentPinInstance.SetActive(true);
            }
            else
            {
                currentPinInstance.transform.SetPositionAndRotation(
                    hitPoint,
                    pinRotation
                );
                if (!currentPinInstance.activeSelf)
                {
                    currentPinInstance.SetActive(true);
                }
            }

            // Draw path to the new pin location
            if (pathDrawer != null)
            {
                Debug.Log($"<color=cyan>[PinPlacer]</color> Calling pathDrawer.DrawPathTo with hitPoint: {hitPoint}");
                pathDrawer.DrawPathTo(hitPoint);
            }
            else
            {
                Debug.LogWarning("<color=red>[PinPlacer]</color> pathDrawer reference is missing!");
            }

            // 推送目的地给机器人 guider
            if (pinDestinationBinder != null)
            {
                // 使用 pin 的 Transform，这样如果以后你让 pin 自己动，guider 也会跟随最新位置
                pinDestinationBinder.SetPinTransform(currentPinInstance.transform);
            }
        }
    }
}