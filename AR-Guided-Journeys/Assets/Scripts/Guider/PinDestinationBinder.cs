using UnityEngine;

/// <summary>
/// Binds the current navigation pin to the GuiderController.
/// - 每次 pin 放置/移动时调用 SetPinTransform。
/// - 在 Update 中把 pin 的世界坐标同步给 GuiderController.SetDestination。
/// - 不修改 GuiderController 里现有的等待/朝向用户等逻辑。
/// </summary>
public class PinDestinationBinder : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Navigation guider robot controller.")]
    public GuiderController guider;

    [Tooltip("当前场景中用作导航目标的 pin Transform（由 PinPlacer 设置）")] 
    public Transform pinTransform;

    [Header("Update Settings")]
    [Tooltip("是否在每帧持续把 pin 位置推送给 GuiderController。一般保持开启。")]
    public bool continuousUpdate = true;

    /// <summary>
    /// 由 PinPlacer 在 pin 创建/移动之后调用，告知最新的 pin Transform。
    /// </summary>
    public void SetPinTransform(Transform pin)
    {
        pinTransform = pin;

        if (guider != null && pinTransform != null)
        {
            guider.SetDestination(pinTransform.position);
        }
    }

    void Update()
    {
        if (!continuousUpdate)
        {
            return;
        }

        if (guider == null || pinTransform == null)
        {
            return;
        }

        guider.SetDestination(pinTransform.position);
    }
}
