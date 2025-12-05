using UnityEngine;

/// <summary>
/// Controls spawning/activating the robot guider along the current pathline when a UI toggle is pressed.
/// Behaviour:
/// - Toggle ON: place robot a small distance in front of the user on the current path and start walking to the pin.
/// - While active: if pathline updates (user移动导致路径变化), robot会尝试贴近这条线行走。
/// - 到达 pin 后: 机器人转身面对 user, 然后消失 (禁用 GameObject)。
/// - 多次点击: 每次 Toggle ON 都会把机器人重置到 user 正前方的 pathline 上。
/// </summary>
public class RobotGuiderToggleController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The robot guider root (GameObject that has GuiderController). Will be activated/deactivated by this script.")]
    public GameObject guiderRoot;

    [Tooltip("The GuiderController component driving robot movement.")]
    public GuiderController guiderController;

    [Tooltip("The PathlineDrawer that draws arrows and has user & pin information.")]
    public PathlineDrawer pathlineDrawer;

    [Tooltip("Transform representing the user/head (same as PathlineDrawer.userTransform).")]
    public Transform userTransform;

    [Tooltip("Existing PinDestinationBinder which holds the current pin transform.")]
    public PinDestinationBinder pinBinder;

    [Header("Spawn Settings")]
    [Tooltip("Initial distance in front of the user when spawning the robot.")]
    public float spawnDistanceFromUser = 0.8f;

    [Tooltip("How strongly the robot is attracted back onto the pathline arrows when off-path.")]
    public float pathFollowStrength = 3f;

    [Tooltip("Distance to pin at which we consider arrival and trigger facing user + hide.")]
    public float arrivalRadius = 0.3f;

    // 内部状态
    bool _isActive;

    void Reset()
    {
        guiderRoot = null;
        guiderController = GetComponentInChildren<GuiderController>();
        pinBinder = FindObjectOfType<PinDestinationBinder>();
    }

    void Awake()
    {
        if (guiderController == null && guiderRoot != null)
        {
            guiderController = guiderRoot.GetComponentInChildren<GuiderController>();
        }

        if (pinBinder == null)
        {
            pinBinder = FindObjectOfType<PinDestinationBinder>();
        }

        if (guiderRoot != null)
        {
            guiderRoot.SetActive(false);
        }
    }

    /// <summary>
    /// 外部 UI Toggle 调用此方法：true = 打开机器人导航, false = 关闭并隐藏。
    /// </summary>
    public void SetRobotEnabled(bool enabled)
    {
        if (enabled)
        {
            ActivateRobot();
        }
        else
        {
            DeactivateRobot();
        }
    }

    void ActivateRobot()
    {
        if (guiderRoot == null || guiderController == null || userTransform == null || pinBinder == null || pinBinder.pinTransform == null)
        {
            Debug.LogWarning("[RobotGuiderToggleController] Missing references, cannot activate robot.");
            return;
        }

        guiderRoot.SetActive(true);

        // 确保控制脚本处于启用状态
        if (!guiderController.enabled)
        {
            guiderController.enabled = true;
        }

        // 将机器人初始化到 user 前方一点距离，并贴在当前 pathline 的高度上
        Vector3 forwardFlat = userTransform.forward;
        forwardFlat.y = 0f;
        if (forwardFlat.sqrMagnitude < 0.0001f)
        {
            forwardFlat = Vector3.forward;
        }
        forwardFlat.Normalize();

        Vector3 spawnPos = userTransform.position + forwardFlat * spawnDistanceFromUser;

        // 可选：将 spawnPos 投影到与 pathline 相同的高度
        if (pathlineDrawer != null)
        {
            spawnPos.y = userTransform.position.y + pathlineDrawer.heightOffset;
        }

        guiderRoot.transform.position = spawnPos;

        // 先让机器人面对 user（reset 时明确朝向人）
        Vector3 toUser = userTransform.position - guiderRoot.transform.position;
        toUser.y = 0f;
        if (toUser.sqrMagnitude > 0.0001f)
        {
            guiderRoot.transform.rotation = Quaternion.LookRotation(toUser.normalized, Vector3.up);
        }

        // 设置 GuiderController 的 user/destination 状态
        guiderController.userTransform = userTransform;
        guiderController.arrivalRadius = arrivalRadius;
        guiderController.SetDestination(pinBinder.pinTransform.position);

        _isActive = true;
    }

    void DeactivateRobot()
    {
        _isActive = false;
        if (guiderRoot != null)
        {
            guiderRoot.SetActive(false);
        }
        if (guiderController != null)
        {
            guiderController.ClearDestination();
        }
    }

    void Update()
    {
        if (!_isActive || guiderRoot == null || guiderController == null)
        {
            return;
        }

        if (pinBinder == null || pinBinder.pinTransform == null)
        {
            // 没有目标时自动隐藏
            DeactivateRobot();
            return;
        }

        // 实时更新 destination（PinDestinationBinder 也可以同时更新，二者等价）
        guiderController.SetDestination(pinBinder.pinTransform.position);

        // 检查是否到达 pin
        Vector3 toPin = pinBinder.pinTransform.position - guiderRoot.transform.position;
        toPin.y = 0f;
        if (toPin.magnitude <= arrivalRadius)
        {
            // 到达：先转身面对 user，稍等一会儿再隐藏
            if (userTransform != null)
            {
                Vector3 dirToUser = userTransform.position - guiderRoot.transform.position;
                dirToUser.y = 0f;
                if (dirToUser.sqrMagnitude > 0.0001f)
                {
                    guiderRoot.transform.rotation = Quaternion.LookRotation(dirToUser.normalized, Vector3.up);
                }
            }

            // 使用协程延迟隐藏，给玩家一个“告别”瞬间
            StartCoroutine(HideAfterDelay(0.5f));
            return;
        }

        // 简单的“贴近 pathline”逻辑：
        // 如果场景中有 pathline（由箭头 Prefabs 实例组成），可以让机器人朝最近的一个箭头略微偏移。
        if (pathlineDrawer != null)
        {
            GameObject nearestArrow = FindNearestArrowToGuider();
            if (nearestArrow != null)
            {
                Vector3 offset = nearestArrow.transform.position - guiderRoot.transform.position;
                offset.y = 0f;
                guiderRoot.transform.position += offset * Mathf.Min(1f, pathFollowStrength * Time.deltaTime);
            }
        }
    }

    GameObject FindNearestArrowToGuider()
    {
        // 简单实现：全场遍历带有同一 prefab 名或特定 tag 的箭头。
        // 建议在 direction_arrows prefab 上加一个 Tag，比如 "PathArrow"。
        GameObject[] arrows = GameObject.FindGameObjectsWithTag("PathArrow");
        if (arrows == null || arrows.Length == 0)
        {
            return null;
        }

        GameObject nearest = null;
        float bestSqr = float.MaxValue;
        Vector3 origin = guiderRoot.transform.position;
        foreach (var a in arrows)
        {
            if (a == null) continue;
            float sqr = (a.transform.position - origin).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                nearest = a;
            }
        }

        return nearest;
    }

    System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        _isActive = false; // 停止 Update 内后续逻辑
        yield return new WaitForSeconds(delay);
        DeactivateRobot();
    }
}
