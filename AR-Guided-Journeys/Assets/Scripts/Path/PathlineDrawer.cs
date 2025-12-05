using System.Collections.Generic;
using UnityEngine;

public class PathlineDrawer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The arrow prefab to instantiate along the line")]
    public GameObject arrowPrefab;

    [Tooltip("Distance between arrows")]
    public float arrowSpacing = 0.5f;

    [Tooltip("Height offset from the ground")]
    public float heightOffset = 0.1f;

    [Tooltip("The user's transform (start of the line)")]
    public Transform userTransform;

    [Header("Arrow Rotation")]
    [Tooltip("Local rotation (Euler) applied to each arrow on top of the path direction. Use this来微调箭头朝向/朝上方向.")]
    public Vector3 arrowLocalEuler = Vector3.zero;

    [Header("Auto Update")]
    [Tooltip("If enabled, the path will be redrawn every frame using the latest user position and last target.")]
    public bool autoUpdate = true;

    private readonly List<GameObject> spawnedArrows = new List<GameObject>();

    // 最近一次 DrawPathTo 记录的目标点；用于 autoUpdate
    private Vector3? _currentTarget;

    void Awake()
    {
        Debug.Log("<color=yellow>[PathlineDrawer]</color> Awake - ready for straight line drawing");
    }

    /// <summary>
    /// Set or update the path target. When autoUpdate is enabled, the
    /// line will be recomputed every frame based on the latest user position.
    /// </summary>
    public void DrawPathTo(Vector3 targetPosition)
    {
        if (userTransform == null)
        {
            Debug.LogWarning("<color=red>[PathlineDrawer]</color> User Transform is not assigned.");
            return;
        }

        if (arrowPrefab == null)
        {
            Debug.LogWarning("<color=red>[PathlineDrawer]</color> Arrow Prefab is not assigned.");
            return;
        }
        _currentTarget = targetPosition;

        // 如果关闭 autoUpdate，就直接画一次；否则让 Update() 每帧负责重绘
        if (!autoUpdate)
        {
            DrawOnce(userTransform.position, targetPosition);
        }
    }

    void Update()
    {
        if (!autoUpdate)
        {
            return;
        }

        if (_currentTarget.HasValue && userTransform != null && arrowPrefab != null)
        {
            DrawOnce(userTransform.position, _currentTarget.Value);
        }
    }

    /// <summary>
    /// 实际执行一次直线路径绘制的函数（不会记录 target，仅根据传入位置重画）。
    /// </summary>
    void DrawOnce(Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        ClearPath();

        Vector3 start = startWorldPos;
        Vector3 end = targetWorldPos;

        // 把起点和终点都抬到同一个高度，避免箭头钻到地里
        float y = Mathf.Min(start.y, end.y) + heightOffset;
        start.y = y;
        end.y = y;

        Vector3 dir = (end - start);
        float length = dir.magnitude;
        if (length < 0.001f)
        {
            Debug.LogWarning("<color=red>[PathlineDrawer]</color> Start and target are too close, skip drawing.");
            return;
        }

        dir.Normalize();

        Debug.Log($"<color=cyan>[PathlineDrawer]</color> DrawOnce, start={start}, end={end}, length={length}");

        float distanceCovered = 0f;
        while (distanceCovered <= length)
        {
            Vector3 position = start + dir * distanceCovered;

            GameObject arrow = Instantiate(arrowPrefab, position, Quaternion.identity);

            // 先按路径方向对齐（前进方向）
            var lookRot = Quaternion.LookRotation(dir, Vector3.up);

            // 再叠加一个在 Inspector 可调的本地旋转，用来把模型“放平”或转成你想要的方向
            var baseRotation = Quaternion.Euler(arrowLocalEuler);

            arrow.transform.rotation = lookRot * baseRotation;

            // 确保实例是激活的（即使 prefab 在场景里是禁用的）
            arrow.SetActive(true);
            spawnedArrows.Add(arrow);

            distanceCovered += arrowSpacing;
        }
    }

    public void ClearPath()
    {
        if (spawnedArrows.Count > 0)
        {
            Debug.Log($"<color=yellow>[PathlineDrawer]</color> ClearPath - destroying {spawnedArrows.Count} arrows.");
        }

        foreach (var arrow in spawnedArrows)
        {
            if (arrow != null)
            {
                Destroy(arrow);
            }
        }

        spawnedArrows.Clear();
    }
}
