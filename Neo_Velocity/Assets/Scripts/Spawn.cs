using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour {
    public bool SpawnGhost = false;

    public GameObject playerPrefab; // Reference to the player prefab

    private GameObject player; // Reference to the spawned player object
    private GameObject playerGhost;
    
    public void Init()
    {
        SpawnPlayer();
        TimeMeasure timeMeasure = player.GetComponent<TimeMeasure>();
        timeMeasure.InitializeTimer();
    }

    void SpawnPlayer() {
        if (SpawnGhost)
        {
            playerGhost = Instantiate(playerPrefab, transform.position, transform.rotation);
            playerGhost.GetComponent<PlayerScript>().MakeGhost();
        }
        player = Instantiate(playerPrefab, transform.position, transform.rotation);
    }
}