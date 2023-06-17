using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   public void QuitGame(){
        Application.Quit();
    }
    public void LevelLoad(){
        SceneManager.LoadScene("LevelSelect");
    }

    public void LoadSettings() {
        SceneManager.LoadScene("Settings");
    }

    public void LoadCredits() {
        SceneManager.LoadScene("Credits");
    }
}
