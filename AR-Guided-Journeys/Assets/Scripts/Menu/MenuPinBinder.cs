using UnityEngine;

/// <summary>
/// 将 dropdown 菜单的选项与空间中的 waypoint/pin 关联：
/// - Hover 某个选项时，在对应 waypoint 位置显示预览 pin；
/// - Click 某个选项时，将正式 pin 移动到 waypoint，并触发 pathline + guider。
/// 场景中始终只有一个真正的 destination pin。
/// </summary>
public class MenuPinBinder : MonoBehaviour
{
    [Header("References")]
    [Tooltip("从 MenuWaypointSpawner 生成的 waypoints。")]
    public Transform[] waypoints;

    [Tooltip("场景中的正式导航 pin（与 PinPlacer / PinDestinationBinder 使用同一个 transform）。")]
    public Transform mainPinTransform;

    [Tooltip("用于 hover 预览的 ghost pin，点击后不会被当作真正 destination。")]
    public Transform previewPinTransform;

    [Tooltip("用于绘制 user->pin path 的 PathlineDrawer。")]
    public PathlineDrawer pathlineDrawer;

    [Tooltip("现有的 PinDestinationBinder，用于把 mainPinTransform 传给 GuiderController。")]
    public PinDestinationBinder pinDestinationBinder;

    void Awake()
    {
        if (previewPinTransform != null)
        {
            previewPinTransform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 下拉菜单 hover 到某个选项时调用。index = -1 表示移出，隐藏 preview。
    /// </summary>
    public void OnItemHover(int index)
    {
        if (previewPinTransform == null)
            return;

        if (index < 0 || waypoints == null || index >= waypoints.Length || waypoints[index] == null)
        {
            previewPinTransform.gameObject.SetActive(false);
            return;
        }

        previewPinTransform.position = waypoints[index].position;
        previewPinTransform.rotation = waypoints[index].rotation;
        previewPinTransform.gameObject.SetActive(true);
    }

    /// <summary>
    /// 下拉菜单点击某个选项时调用，正式将导航 pin 放到对应 waypoint。
    /// </summary>
    public void OnItemClick(int index)
    {
        if (waypoints == null || index < 0 || index >= waypoints.Length || waypoints[index] == null)
        {
            Debug.LogWarning("[MenuPinBinder] Invalid waypoint index " + index);
            return;
        }

        if (mainPinTransform == null)
        {
            Debug.LogWarning("[MenuPinBinder] mainPinTransform is null.");
            return;
        }

        Vector3 pos = waypoints[index].position;
        Quaternion rot = waypoints[index].rotation;

        mainPinTransform.position = pos;
        mainPinTransform.rotation = rot;

        // 关闭 preview
        if (previewPinTransform != null)
        {
            previewPinTransform.gameObject.SetActive(false);
        }

        // 更新路径
        if (pathlineDrawer != null)
        {
            pathlineDrawer.DrawPathTo(pos);
        }

        // 通知 guider：保持使用唯一的 main pin 作为 destination
        if (pinDestinationBinder != null)
        {
            pinDestinationBinder.pinTransform = mainPinTransform;
        }
    }
}
