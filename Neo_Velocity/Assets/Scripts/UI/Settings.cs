using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Linq;

public class Settings : AInputScript {
    [SerializeField] AudioSource[] audioSource;
    [SerializeField] Slider audioSlider;
    [SerializeField] Toggle checkbox;
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    void Start() {
        if (audioSlider == null)
            return;
        audioSlider.value = SaveDataManager.Volume;

        checkbox.isOn = SaveDataManager.Fullscreen;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++) {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
        }
        
        resolutionDropdown.AddOptions(options);
    }
    public override Dictionary<string, float> GetInput()
    {
        return new Dictionary<string, float>
        {
            { "Sliding", 0 },
            { "Jump", 0 },
            { "Respawn", 0 },
            { "Mouse X", 7 },
            { "Mouse Y", 0 },
            { "Vertical", 1 },
            { "Horizontal", 0 },
            { "Shoot", 0 },
            { "Activate", 0 },
            { "Change", 0 }
        };
    }

    public override Vector2 GetMouseInput()
    {
        return Vector2.zero;
    }

    public override void ResetInputs()
    {
        return;
    }

    public void SetVolume(float volume) {
        //audioMixer.SetFloat("volume", volume);
        audioSource.ToList().ForEach(a => a.volume = volume);
        GameObject.FindGameObjectsWithTag("Music").ToList().ForEach(o => o.GetComponent<AudioSource>().volume = volume);
        SaveDataManager.Volume = volume;
    }

    public void SetQuality(int qualityIndex) {
        QualitySettings.SetQualityLevel(qualityIndex);
        SaveDataManager.Quality = qualityIndex;
    }

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
        SaveDataManager.Fullscreen = isFullscreen;
    }

    public void Back() {
        SceneManager.LoadScene("Main_Menu");
    }

    public void Inputs()
    {
        SceneManager.LoadScene("Input");
    }
}
