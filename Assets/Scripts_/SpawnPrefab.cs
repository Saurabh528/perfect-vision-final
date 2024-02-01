using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    public GameObject prefab; // The prefab to be spawned
    public float spawnInterval; // The time interval between each spawn
    public float spawnDelay; // The time delay before the first spawn
    public float spawnHeight; // The height at which the prefab will be spawned

    // Start is called before the first frame update
    void Start()
    {
        // Start spawning the prefab after the specified delay
        InvokeRepeating("Spawn", spawnDelay, spawnInterval);
    }

    // Spawn the prefab
    void Spawn()
    {
        // Calculate the spawn position at the bottom of the screen
        Vector3 spawnPosition = new Vector3(Random.Range(-5f, 5f), spawnHeight, 0);

        // Spawn the prefab
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}
