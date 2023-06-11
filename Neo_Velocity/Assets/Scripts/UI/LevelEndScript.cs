using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelEndScript : MonoBehaviour {
    [SerializeField] private string nextLevelName;
    [SerializeField] private Button Nextbutton;
    [SerializeField] private TMP_Text Nexttext;
 

    public void Start() {
        
        if (nextLevelName == "") {
            Nextbutton.enabled = false;
            Nexttext.enabled = false;
        }
    }
    

    
    public void RenameReplay(TMP_InputField Input) {
        string Name = Input.text;
        GameObject.FindGameObjectsWithTag("Player").First(o => !o.GetComponent<PlayerScript>().IsGhost).GetComponent<PlayerInputScript>().RenameCurrentFile(Name);
    }
    public void StayInLevel()
    {
        GameObject.FindGameObjectsWithTag("Player").ToList().ForEach(o => o.GetComponent<PlayerScript>().Respawn = true);
    }
    public void LoadNextLevel() {
        LoadScene(nextLevelName);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Main_Menu");
    }

    // From LevelSelect
    public void LoadScene(string sceneName)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName);
        operation.completed += o => { GameObject.FindGameObjectWithTag("Spawn").GetComponent<Spawn>().Init(); };
    }
}
