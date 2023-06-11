using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndScript : MonoBehaviour
{
    public void RenameReplay(string Name)
    {
        GameObject.FindGameObjectsWithTag("Player").First(o => !o.GetComponent<PlayerScript>().IsGhost).GetComponent<PlayerInputScript>().RenameCurrentFile(Name);
    }
    public void StayInLevel()
    {
        GameObject.FindGameObjectsWithTag("Player").ToList().ForEach(o => o.GetComponent<PlayerScript>().Respawn = true);
    }
    public void LoadNextLevel(string SceneName)
    {
        LoadScene(SceneName);
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
