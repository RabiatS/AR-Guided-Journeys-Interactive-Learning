using UnityEngine;

/// <summary>
/// 在场景中生成与菜单选项数量对应的一组 waypoint，用于 dropdown 选项导航。
/// 这里只是简单在玩家前方一定范围内随机采样平面位置，你也可以之后改成 MRUK 射线到 FLOOR。
/// </summary>
public class MenuWaypointSpawner : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("要生成的 waypoint 数量，应与 dropdown 选项数一致。")]
    public int waypointCount = 4;

    [Tooltip("生成区域的半径（以 userTransform 为中心的圆形平面）。")]
    public float radius = 2.0f;

    [Tooltip("参考的用户 Transform（通常是 CenterEyeAnchor），作为随机区域中心。")]
    public Transform userTransform;

    [Header("Runtime Output")]
    [Tooltip("生成好的 waypoint transforms，供其他脚本（如 MenuPinBinder）使用。")]
    public Transform[] waypoints;

    void Start()
    {
        if (waypointCount <= 0)
            return;

        if (userTransform == null)
        {
            Debug.LogWarning("[MenuWaypointSpawner] userTransform is null, using own transform as center.");
        }

        waypoints = new Transform[waypointCount];

        Vector3 center = userTransform != null ? userTransform.position : transform.position;
        Vector3 forwardFlat = userTransform != null ? userTransform.forward : transform.forward;
        forwardFlat.y = 0f;
        if (forwardFlat.sqrMagnitude < 0.0001f)
            forwardFlat = Vector3.forward;
        forwardFlat.Normalize();

        // 让 waypoint 大致分布在玩家前方扇形区域
        for (int i = 0; i < waypointCount; i++)
        {
            float angle = Mathf.Lerp(-45f, 45f, (waypointCount == 1) ? 0.5f : (float)i / (waypointCount - 1));
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rot * forwardFlat;
            float dist = radius * 0.7f + Random.Range(-radius * 0.2f, radius * 0.2f);

            Vector3 pos = center + dir * dist;
            pos.y = center.y; // 先放在与 user 同高的平面上

            GameObject wp = new GameObject($"MenuWaypoint_{i}");
            wp.transform.SetParent(transform);
            wp.transform.position = pos;
            wp.transform.rotation = Quaternion.identity;
            waypoints[i] = wp.transform;
        }
    }
}
