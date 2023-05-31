using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject playerPrefab; // Reference to the player prefab

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        GameObject player = Instantiate(playerPrefab, transform.position, transform.rotation);
    }
}
