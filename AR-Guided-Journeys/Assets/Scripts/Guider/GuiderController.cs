using UnityEngine;

/// <summary>
/// Simple navigation guider behaviour for the corridor robot.
/// - On start: face the user and do a small left-right wobble.
/// - After destination is selected: turn to face the destination.
/// - While the user moves: robot moves towards the destination (no pathfinding yet).
/// - If the user stays still for a while: robot turns back to face the user.
/// This script does NOT decide the destination; another script should call
/// SetDestination when the user picks a door.
/// </summary>
public class GuiderController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform representing the user/head. Typically XR camera or HMD.")]
    public Transform userTransform;

    [Tooltip("Optional: visual root of the guider for rotation (if different from this transform).")]
    public Transform guiderVisualRoot;

    [Header("Movement")]
    [Tooltip("Linear speed when walking towards destination (m/s).")]
    public float moveSpeed = 1.2f;

    [Tooltip("How quickly the guider rotates towards a target direction (deg/s).")]
    public float rotationSpeed = 180f;

    [Tooltip("Approximate center X position of the corridor. Robot lateral offset will be clamped around this.")]
    public float corridorCenterX;

    [Tooltip("Maximum sideways distance from the corridor center line.")]
    public float maxLateralOffset = 0.25f;

    [Tooltip("Radius around destination within which the guider stops walking.")]
    public float arrivalRadius = 0.3f;

    [Header("Idle Facing User")] 
    [Tooltip("Seconds the user must be still before the guider turns to face the user again.")]
    public float userIdleSeconds = 3f;

    [Tooltip("Threshold (m) under which user motion is considered 'still'.")]
    public float userMovementThreshold = 0.03f;

    [Header("Start Wobble Animation")] 
    [Tooltip("Total angle in degrees for the left-right wobble.")]
    public float wobbleAngle = 25f;

    [Tooltip("Duration in seconds for a single wobble cycle (left+right).")]
    public float wobbleDuration = 1.2f;

    [Tooltip("Number of wobble cycles to play when the scene starts.")]
    public int wobbleCycles = 2;

    [Header("State")] 
    [Tooltip("Has the user already selected a destination? Set via SetDestination.")]
    public bool destinationSelected;

    [Tooltip("Current destination point that the guider should walk towards.")]
    public Vector3 destinationPosition;

    [Header("Debug")]
    [Tooltip("Log basic movement info each second to diagnose navigation.")]
    public bool debugMovement;

    Transform RotationRoot => guiderVisualRoot != null ? guiderVisualRoot : transform;

    Vector3 _lastUserPosition;
    float _idleTimer;
    bool _isWobbling;
    float _wobbleTime;
    int _completedWobbles;
    Quaternion _wobbleBaseRotation;
    float _debugTimer;

    void Start()
    {
        if (userTransform != null)
        {
            RotationRoot.rotation = Quaternion.LookRotation(GetFlatDirectionTo(userTransform.position));
            _lastUserPosition = userTransform.position;
        }

        _wobbleBaseRotation = RotationRoot.rotation;
        _isWobbling = true;
        _wobbleTime = 0f;
        _completedWobbles = 0;

        // If corridor center is not set in inspector, initialise with current X.
        if (Mathf.Approximately(corridorCenterX, 0f))
        {
            corridorCenterX = transform.position.x;
        }
    }

    void Update()
    {
        UpdateUserIdleTimer();

        if (_isWobbling)
        {
            UpdateStartWobble();
        }
        else
        {
            UpdateBehaviour();
        }
    }

    void UpdateUserIdleTimer()
    {
        if (userTransform == null)
        {
            return;
        }

        float distance = Vector3.Distance(userTransform.position, _lastUserPosition);
        if (distance > userMovementThreshold)
        {
            _idleTimer = 0f;
            _lastUserPosition = userTransform.position;
        }
        else
        {
            _idleTimer += Time.deltaTime;
        }
    }

    void UpdateStartWobble()
    {
        if (userTransform != null)
        {
            RotationRoot.rotation = Quaternion.Lerp(
                RotationRoot.rotation,
                Quaternion.LookRotation(GetFlatDirectionTo(userTransform.position)),
                Time.deltaTime * 8f);
            _wobbleBaseRotation = RotationRoot.rotation;
        }

        _wobbleTime += Time.deltaTime;
        float cycleTime = Mathf.Max(0.1f, wobbleDuration);
        float normalized = Mathf.Clamp01(_wobbleTime / cycleTime);

        float wobbleOffset = Mathf.Sin(normalized * Mathf.PI * 2f) * wobbleAngle;
        Quaternion offsetRot = Quaternion.AngleAxis(wobbleOffset, Vector3.up);
        RotationRoot.rotation = _wobbleBaseRotation * offsetRot;

        if (_wobbleTime >= cycleTime)
        {
            _wobbleTime = 0f;
            _completedWobbles++;
            if (_completedWobbles >= Mathf.Max(0, wobbleCycles))
            {
                _isWobbling = false;
                RotationRoot.rotation = _wobbleBaseRotation;
            }
        }
    }

    void UpdateBehaviour()
    {
        if (destinationSelected)
        {
            Vector3 toDestination = GetFlatDirectionTo(destinationPosition);

            // Distance to destination in the horizontal plane.
            Vector3 planarToDest = destinationPosition - transform.position;
            planarToDest.y = 0f;
            float distanceToDest = planarToDest.magnitude;

            // 仅在到达半径内才停下面向用户，否则持续向目的地前进
            if (distanceToDest <= arrivalRadius && userTransform != null)
            {
                Vector3 toUser = GetFlatDirectionTo(userTransform.position);
                RotateTowards(toUser);
            }
            else
            {
                RotateTowards(toDestination);
                MoveTowardsDestination();
            }

            if (debugMovement)
            {
                _debugTimer += Time.deltaTime;
                if (_debugTimer >= 1f)
                {
                    _debugTimer = 0f;
                    float planarDist = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), new Vector3(destinationPosition.x, 0f, destinationPosition.z));
                    Debug.Log($"[GuiderController] Moving; dist={planarDist:F2}, destSelected={destinationSelected}, dest={destinationPosition}");
                }
            }
        }
        else if (userTransform != null)
        {
            Vector3 toUser = GetFlatDirectionTo(userTransform.position);
            RotateTowards(toUser);
        }
    }

    void MoveTowardsDestination()
    {
        Vector3 toDest = destinationPosition - transform.position;
        toDest.y = 0f;
        float distance = toDest.magnitude;
        if (distance <= 0.01f)
        {
            return;
        }

        Vector3 direction = toDest / distance;
        float step = moveSpeed * Time.deltaTime;
        step = Mathf.Min(step, distance);
        Vector3 newPosition = transform.position + direction * step;

        // Constrain sideways motion so the guider stays near the corridor center line.
        if (maxLateralOffset > 0f)
        {
            float targetX = Mathf.Clamp(newPosition.x, corridorCenterX - maxLateralOffset, corridorCenterX + maxLateralOffset);
            newPosition.x = targetX;
        }

        transform.position = newPosition;
    }

    void RotateTowards(Vector3 flatDirection)
    {
        if (flatDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion target = Quaternion.LookRotation(flatDirection, Vector3.up);
        RotationRoot.rotation = Quaternion.RotateTowards(
            RotationRoot.rotation,
            target,
            rotationSpeed * Time.deltaTime);
    }

    Vector3 GetFlatDirectionTo(Vector3 worldTarget)
    {
        Vector3 origin = RotationRoot.position;
        Vector3 direction = worldTarget - origin;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return RotationRoot.forward;
        }
        return direction.normalized;
    }

    /// <summary>
    /// Call this when the user picks a destination (for example, a door waypoint).
    /// </summary>
    public void SetDestination(Vector3 worldPosition)
    {
        destinationSelected = true;
        destinationPosition = worldPosition;
    }

    /// <summary>
    /// Optional: clear destination and go back to just facing the user.
    /// </summary>
    public void ClearDestination()
    {
        destinationSelected = false;
    }
}
