using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public GameObject SceneDisplay;
    public GameObject PlayerPrefab;
    public GameObject ReplayInfoHolder;
    public GameObject UIReplayPrefab;

    string CurrentLevel;

    private void Start()
    {
        CurrentLevel = null;
    }

    public void LevelButtonPressed(string name)
    {
        CurrentLevel = name;
        SceneDisplay.GetComponent<TextMeshProUGUI>().text = name.Replace("_", " ");
        List<string> allReplays = GetReplayNames(name);
        ReplayInfoHolder.transform.DetachChildren();
        ReplayInfoHolder.GetComponent<RectTransform>().sizeDelta =
            new Vector2(ReplayInfoHolder.GetComponent<RectTransform>().sizeDelta.x, 80 * allReplays.Count);
        int i = 0;
        allReplays.ForEach(r =>
        {
            GameObject temp = Instantiate(UIReplayPrefab);
            temp.GetComponent<UIReplayInfoHolderScript>().Text.GetComponent<TextMeshProUGUI>().text = r.Remove(r.Length-7);
            temp.transform.SetParent(ReplayInfoHolder.transform);
            if (i == 0) {
                temp.GetComponent<RectTransform>().localPosition = new Vector2(90, -40);
            }
            else {
                temp.GetComponent<RectTransform>().localPosition = new Vector2(90, -80 * i -40); // This doesn't correctly arrange the Buttons, good Luck!
            }
            i++;
            GameObject GhostButton = temp.GetComponent<UIReplayInfoHolderScript>().GhostButton;
            GhostButton.GetComponent<UIReplayButtonScript>().value = r;
            GhostButton.GetComponent<UIReplayButtonScript>().action = GhostButtonPressed;
            GhostButton.GetComponent<Button>().onClick
            .AddListener(() => GhostButton.GetComponent<UIReplayButtonScript>().Execute());
            GameObject WatchButton = temp.GetComponent<UIReplayInfoHolderScript>().WatchButton;
            WatchButton.GetComponent<UIReplayButtonScript>().value = r;
            WatchButton.GetComponent<UIReplayButtonScript>().action = WatchButtonPressed;
            WatchButton.GetComponent<Button>().onClick
            .AddListener(() => WatchButton.GetComponent<UIReplayButtonScript>().Execute());
        });
    }

    public void PlayButtonPressed()
    {
        if (CurrentLevel == null)
            return;
        LoadScene(CurrentLevel);
    }

    public void GhostButtonPressed(string replay)
    {
        if (CurrentLevel == null)
            return;
        SetupVSGhost(CurrentLevel, replay);
    }

    public void WatchButtonPressed(string replay)
    {
        if (CurrentLevel == null)
            return;
        SetupWatchGhost(CurrentLevel, replay);
    }

    public void LoadScene(string sceneName) {
        var operation = SceneManager.LoadSceneAsync(sceneName);
        operation.completed += o => { GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().Init(); };
    }

    void SetupVSGhost(string sceneName, string Replay)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName);
        operation.completed += o =>
        {
            GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().SpawnGhost = true;
            PlayerPrefab.GetComponent<ReplayInputScript>().ReplayName = Replay;
            GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().Init();
        };
    }

    void SetupWatchGhost(string sceneName, string Replay)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName);
        operation.completed += o =>
        {
            GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().SpawnGhost = true;
            GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().WatchGhost = true;
            PlayerPrefab.GetComponent<ReplayInputScript>().ReplayName = Replay;
            GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().Init();
        };
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
            StreamReader sr = new StreamReader(p);
            sr.ReadLine();
            sr.ReadLine();
            string temp = sr.ReadLine();
            sr.Close();
            return temp == SceneName;
        }).Select(s => s.Substring(ReplayFolderLocation.Length)).ToList();
    }
    public void LoadMainMenu() {
        SceneManager.LoadScene("Main_Menu");
    }
}
