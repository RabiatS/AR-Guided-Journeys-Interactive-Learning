using UnityEngine;

public class DestinationSpawner : MonoBehaviour
{
    public Transform destinationCube;
    public float spawnDistance = 2f; // 2 meters in front

    void Start()
    {
        if (destinationCube != null && Camera.main != null)
        {
            // Spawn cube in front of user at floor level
            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * spawnDistance;
            spawnPos.y = 0f; // Floor level
            destinationCube.position = spawnPos;
        }
    }
}
