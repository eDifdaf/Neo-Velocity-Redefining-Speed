using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour {
    public GameObject playerPrefab; // Reference to the player prefab

    private GameObject player; // Reference to the spawned player object

    void Start() {
        SpawnPlayer();
        TimeMeasure timeMeasure = player.GetComponent<TimeMeasure>();
        timeMeasure.InitializeTimer();
    }

    void SpawnPlayer() {
        player = Instantiate(playerPrefab, transform.position, transform.rotation);
    }
}