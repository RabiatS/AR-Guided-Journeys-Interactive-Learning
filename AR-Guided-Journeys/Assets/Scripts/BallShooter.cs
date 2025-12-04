using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Controller anchor used as the muzzle")] public Transform controllerMuzzle;
    [Tooltip("Prefab with SphereCollider + Rigidbody used as the projectile")] public GameObject ballPrefab;

    [Header("Ball Physics")]
    [Tooltip("Forward impulse applied to each ball")] public float launchForce = 8f;
    [Tooltip("Seconds before auto-destroying the spawned ball")] public float ballLifetime = 8f;
    [Tooltip("Minimum delay between two shots")] public float fireCooldown = 0.2f;

    private float nextFireTime;

    private void Update()
    {
        if (controllerMuzzle == null || ballPrefab == null)
        {
            return;
        }

        if (Time.unscaledTime < nextFireTime)
        {
            return;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        GameObject ballInstance = Instantiate(ballPrefab, controllerMuzzle.position, controllerMuzzle.rotation);

        Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = ballInstance.AddComponent<Rigidbody>();
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(controllerMuzzle.forward * launchForce, ForceMode.VelocityChange);

        if (ballLifetime > 0f)
        {
            Destroy(ballInstance, ballLifetime);
        }

        nextFireTime = Time.unscaledTime + fireCooldown;
    }
}
