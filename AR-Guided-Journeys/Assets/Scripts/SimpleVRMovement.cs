using UnityEngine;

public class SimpleVRMovement : MonoBehaviour
{
    public float speed = 3f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
       
        float h = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
        float v = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move * speed * Time.deltaTime);
    }
}
