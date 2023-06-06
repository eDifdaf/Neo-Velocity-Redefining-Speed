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
        ReplayInfoHolder.GetComponent<RectTransform>().sizeDelta =
            new Vector2(ReplayInfoHolder.GetComponent<RectTransform>().sizeDelta.x, 80 * allReplays.Count);
        Debug.Log(allReplays.Count);
        int i = 0;
        allReplays.ForEach(r =>
        {
            GameObject temp = Instantiate(UIReplayPrefab);
            temp.GetComponent<UIReplayInfoHolderScript>().Text.GetComponent<TextMeshProUGUI>().text = r;
            temp.transform.SetParent(ReplayInfoHolder.transform);
            temp.GetComponent<RectTransform>().localPosition = new Vector2(90, -80 * i); // This doesn't correctly arrange the Buttons, good Luck!
            temp.GetComponent<UIReplayInfoHolderScript>().GhostButton.GetComponent<Button>().onClick
            .AddListener(temp.GetComponent<UIReplayInfoHolderScript>().GhostButton.GetComponent<UIReplayButtonScript>().Execute);
            temp.GetComponent<UIReplayInfoHolderScript>().GhostButton.GetComponent<UIReplayButtonScript>().value = r;
            temp.GetComponent<UIReplayInfoHolderScript>().GhostButton.GetComponent<UIReplayButtonScript>().action = GhostButtonPressed;
            i++;
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
        }).ToList();
    }
}
