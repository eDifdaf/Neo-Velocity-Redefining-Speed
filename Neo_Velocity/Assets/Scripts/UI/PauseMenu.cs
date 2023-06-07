using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused = false;
    public GameObject pauseUI;

    // Update is called once per frame
    void Update()    {
        if (Input.GetKeyDown(KeyCode.Escape)){
            if (IsPaused){
                Resume();
            }
            else{
            Pause();
            }
        }

    }
    public void Resume(){
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Pause(){
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LoadMenu(){
        Time.timeScale = 1f;
        SceneManager.LoadScene("Settings");
    }
    public void QuitGame(){
		Time.timeScale = 1f;
        SceneManager.LoadScene("Main_Menu");
    }
}
