using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayLoadingScript : MonoBehaviour
{
    public GameObject PlayerPrefab;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void SetupVSGhost(string SceneName, string Replay)
    {
        SceneManager.LoadScene(SceneName);
        GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().SpawnGhost = true;
        PlayerPrefab.GetComponent<ReplayInputScript>().ReplayName = Replay;
        GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().Init();
    }

    float? GetReplayTime(string Replay)
    {
        StreamReader sr = new StreamReader(PlayerPrefab.GetComponent<ReplayInputScript>().ReplayFolderLocation + Replay);
        sr.ReadLine();
        string temp = sr.ReadLine();
        sr.Close();
        if (temp == "Not Finished")
            return null;
        return float.Parse(temp, CultureInfo.InvariantCulture);
    }

    List<string> GetReplayNames(string SceneName)
    {
        string ReplayFolderLocation = PlayerPrefab.GetComponent<ReplayInputScript>().ReplayFolderLocation;
        return Directory.GetFiles(ReplayFolderLocation, "*.replay").Where(p =>
        {
            StreamReader sr = new StreamReader(ReplayFolderLocation + p);
            sr.ReadLine();
            sr.ReadLine();
            string temp = sr.ReadLine();
            sr.Close();
            return temp == SceneName;
        }).ToList();
    }
}
