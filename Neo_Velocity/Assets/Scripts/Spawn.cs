using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour {
    public bool SpawnGhost = false;
    public bool WatchGhost = false;

    public GameObject playerPrefab; // Reference to the player prefab

    private GameObject player; // Reference to the spawned player object
    private GameObject playerGhost;
    
    public void Init()
    {
        SpawnPlayer();
        TimeMeasure timeMeasure = player.GetComponent<TimeMeasure>();
        timeMeasure.InitializeTimer();
        GameObject.FindGameObjectWithTag("DataHandler").GetComponent<SaveDataManager>().ApplyCurrentConfig();
    }

    void SpawnPlayer() {
        if (SpawnGhost)
        {
            playerGhost = Instantiate(playerPrefab);
            playerGhost.GetComponent<PlayerScript>().Respawn = true;
            if (WatchGhost)
            {
                playerGhost.GetComponent<PlayerScript>().WatchGhost();
                player = playerGhost;
                return;
            }
            playerGhost.GetComponent<PlayerScript>().MakeGhost();
        }
        player = Instantiate(playerPrefab);
        player.GetComponent<PlayerScript>().Respawn = true;
    }
}