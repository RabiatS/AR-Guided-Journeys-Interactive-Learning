using UnityEngine;

public class FloatingRotatingArrow : MonoBehaviour
{
    public float rotateSpeed = 60f;
    public float floatAmplitude = 0.1f;
    public float floatFrequency = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // rotate
        transform.Rotate(Vector3.right * rotateSpeed * Time.deltaTime);

        // float
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}
