using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class Settings : AInputScript {
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    void Start() {
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
        audioMixer.SetFloat("volume", volume);
    }

    public void SetQuality(int qualityIndex) {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
    }
}
