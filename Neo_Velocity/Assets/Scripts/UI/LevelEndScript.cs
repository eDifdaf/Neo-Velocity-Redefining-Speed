using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelEndScript : MonoBehaviour {
    static public bool CurrentlyDisplayed = false;

    [SerializeField] private string nextLevelName;
    [SerializeField] private GameObject Holder;
    [SerializeField] private Button Nextbutton;
    [SerializeField] private Button SaveReplayButton;
 
    public void Start() {
        if (nextLevelName == "") {
            Nextbutton.interactable = false;
        }
    }
    
    public void Show()
    {
        CurrentlyDisplayed= true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
        Holder.SetActive(true);
        SaveReplayButton.interactable = true;
    }
    public void Hide()
    {
        CurrentlyDisplayed= false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        Holder.SetActive(false);
    }

    
    public void RenameReplay(TMP_InputField Input) {
        string Name = Input.text;
        GameObject.FindGameObjectsWithTag("Player").First(o => !o.GetComponent<PlayerScript>().IsGhost).GetComponent<PlayerInputScript>().RenameCurrentFile(Name);
        SaveReplayButton.interactable = false;

    }
    public void StayInLevel()
    {
        Hide();
        GameObject.FindGameObjectsWithTag("Player").ToList().ForEach(o => o.GetComponent<PlayerScript>().Respawn = true);
    }
    public void LoadNextLevel() {
        Hide();
        LoadScene(nextLevelName);
    }
    public void MainMenu()
    {
        Hide();
        SceneManager.LoadScene("Main_Menu");
    }

    // From LevelSelect
    public void LoadScene(string sceneName)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName);
        operation.completed += o => { GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().Init(); };
    }

    public void OnDestroy()
    {
        Hide();
        Cursor.lockState = CursorLockMode.None;
    }
}
