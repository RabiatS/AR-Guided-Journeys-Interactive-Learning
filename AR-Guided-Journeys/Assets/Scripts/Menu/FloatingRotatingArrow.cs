using UnityEngine;

public class FloatingRotatingArrow : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Axis to rotate around (local space)")] public Vector3 rotationAxis = Vector3.up;
    public float rotateSpeed = 60f;

    [Header("Floating")]
    public float floatAmplitude = 0.1f;
    public float floatFrequency = 2f;

    private Vector3 _startLocalPos;

    void Start()
    {
        _startLocalPos = transform.localPosition;
    }

    void Update()
    {
        Vector3 axis = rotationAxis.sqrMagnitude > 0.0001f ? rotationAxis.normalized : Vector3.up;
        transform.Rotate(axis * rotateSpeed * Time.deltaTime, Space.Self);

        float offset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.localPosition = _startLocalPos + Vector3.up * offset;
    }
}
