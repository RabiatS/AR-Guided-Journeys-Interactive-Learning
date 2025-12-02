using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First navigation prototype: joystick-driven vertical and forward/back movement with grip speed modifiers.
/// Attach this to the rig root and assign the input actions via inspector.
/// </summary>
public class JoystickNavigationController : MonoBehaviour
{
    [Header("Rig References")]
    public Transform rigAnchor;        // Moved transform (typically XR Origin/Camera Offset)
    public Transform rigTracker;       // Forward reference (typically HMD or camera)

    [Header("Input Actions")]
    public InputActionReference leftJoystick;   // Expect a Vector2 (Left controller primary 2D axis)
    public InputActionReference rightJoystick;  // Expect a Vector2 (Right controller primary 2D axis)
    public InputActionReference leftGrip;       // Expect a Button/Float action for the left grip
    public InputActionReference rightGrip;      // Expect a Button/Float action for the right grip

    [Header("Movement Settings")]
    public float baseMoveSpeed = 1.5f;          // Default cruising speed in m/s
    public float maxMoveSpeed = 3.0f;           // Upper bound when accelerating with right grip
    public float minMoveSpeed = 0.5f;           // Lower bound when slowing with left grip
    public float accelerateRate = 2.0f;         // Units per second squared when speeding up
    public float decelerateRate = 2.5f;         // Units per second squared when slowing down
    public float joystickDeadzone = 0.1f;       // Ignore minor drift on joysticks (0..1)
    public float wallBuffer = 0.2f;             // Stop short of colliders by this distance
    public LayerMask moveLayerMask = ~0;        // Colliders that block movement

    float currentSpeed;

    void OnEnable()
    {
        EnableAction(leftJoystick);
        EnableAction(rightJoystick);
        EnableAction(leftGrip);
        EnableAction(rightGrip);

        currentSpeed = Mathf.Clamp(baseMoveSpeed, minMoveSpeed, maxMoveSpeed);
    }

    void OnDisable()
    {
        DisableAction(leftJoystick);
        DisableAction(rightJoystick);
        DisableAction(leftGrip);
        DisableAction(rightGrip);
    }

    void Update()
    {
        if (!rigAnchor)
        {
            return;
        }

        UpdateSpeed();
        float scaledSpeed = currentSpeed * Time.deltaTime;

        Vector2 leftAxis = ReadVector2(leftJoystick);
        Vector2 rightAxis = ReadVector2(rightJoystick);

        Vector3 displacement = Vector3.zero;
        Vector3 rightDir = GetHorizontalRight();

        Vector2 processedLeft = ApplyDeadzone(leftAxis);
        if (processedLeft != Vector2.zero)
        {
            Vector3 leftMove = (rightDir * processedLeft.x) + (Vector3.up * processedLeft.y);
            displacement += leftMove * scaledSpeed;
        }

        Vector2 processedRight = ApplyDeadzone(rightAxis);
        if (processedRight != Vector2.zero)
        {
            Vector3 forwardDir = GetHorizontalForward();
            Vector3 planarMove = (forwardDir * processedRight.y) + (rightDir * processedRight.x);
            displacement += planarMove * scaledSpeed;
        }

        if (displacement.sqrMagnitude > 0f)
        {
            TryMove(displacement);
        }
    }

    void UpdateSpeed()
    {
        float targetSpeed = Mathf.Clamp(baseMoveSpeed, minMoveSpeed, maxMoveSpeed);

        if (ReadTrigger(rightGrip) > 0.5f)
        {
            targetSpeed = maxMoveSpeed;
        }
        else if (ReadTrigger(leftGrip) > 0.5f)
        {
            targetSpeed = minMoveSpeed;
        }

        float rate = targetSpeed > currentSpeed ? accelerateRate : decelerateRate;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Mathf.Max(0f, rate) * Time.deltaTime);
    }

    Vector2 ReadVector2(InputActionReference actionReference)
    {
        return actionReference == null ? Vector2.zero : actionReference.action.ReadValue<Vector2>();
    }

    float ReadTrigger(InputActionReference actionReference)
    {
        return actionReference == null ? 0f : actionReference.action.ReadValue<float>();
    }

    Vector3 GetHorizontalForward()
    {
        Transform forwardReference = rigTracker ? rigTracker : rigAnchor;
        Vector3 forward = forwardReference.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
    }

    Vector3 GetHorizontalRight()
    {
        Transform forwardReference = rigTracker ? rigTracker : rigAnchor;
        Vector3 right = forwardReference.right;
        right.y = 0f;
        return right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;
    }

    void TryMove(Vector3 displacement)
    {
        if (displacement.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 direction = displacement.normalized;
        float distance = displacement.magnitude;

        if (Physics.Raycast(rigAnchor.position, direction, out RaycastHit hit, distance + wallBuffer, moveLayerMask, QueryTriggerInteraction.Ignore))
        {
            float allowedDistance = Mathf.Max(0f, hit.distance - wallBuffer);
            rigAnchor.position += direction * Mathf.Min(allowedDistance, distance);
        }
        else
        {
            rigAnchor.position += displacement;
        }
    }

    Vector2 ApplyDeadzone(Vector2 axis)
    {
        float magnitude = axis.magnitude;
        if (magnitude <= joystickDeadzone)
        {
            return Vector2.zero;
        }

        float denominator = 1f - joystickDeadzone;
        float adjustedMagnitude = denominator > Mathf.Epsilon ? Mathf.Clamp01((magnitude - joystickDeadzone) / denominator) : 1f;
        return axis.normalized * adjustedMagnitude;
    }

    void OnValidate()
    {
        baseMoveSpeed = Mathf.Max(0f, baseMoveSpeed);
        maxMoveSpeed = Mathf.Max(baseMoveSpeed, maxMoveSpeed);
        minMoveSpeed = Mathf.Clamp(minMoveSpeed, 0f, baseMoveSpeed);
        accelerateRate = Mathf.Max(0f, accelerateRate);
        decelerateRate = Mathf.Max(0f, decelerateRate);
        joystickDeadzone = Mathf.Clamp01(joystickDeadzone);
        wallBuffer = Mathf.Max(0f, wallBuffer);
    }

    static void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null)
        {
            actionReference.action.Enable();
        }
    }

    static void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null)
        {
            actionReference.action.Disable();
        }
    }
}