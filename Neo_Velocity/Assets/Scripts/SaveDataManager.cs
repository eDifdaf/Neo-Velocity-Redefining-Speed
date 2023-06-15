using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveDataManager : MonoBehaviour
{
    static string dataPath = "Data\\";
    static string fileName = "config.txt";

    #region fields
    public static KeyCode ForwardKey = KeyCode.W;
    public static KeyCode BackwardsKey = KeyCode.S;
    public static KeyCode LeftKey = KeyCode.A;
    public static KeyCode RightKey = KeyCode.D;
    public static KeyCode SlideKey = KeyCode.LeftControl;
    public static KeyCode JumpKey = KeyCode.Space;
    public static KeyCode RespawnKey = KeyCode.R;
    public static KeyCode ShootKey = KeyCode.Mouse0;
    public static KeyCode ActivateKey = KeyCode.Mouse1;
    public static bool UseChangeKey = false;
    public static KeyCode ChangeKey = KeyCode.Mouse2;
    public static float VerticalMouseSensitivity = 4f;
    public static float HorizontalMouseSensitivity = 7f;
    public static bool UseFixedDistanceLookKeys = true;
    public static KeyCode UpLookKey = KeyCode.I;
    public static KeyCode DownLookKey = KeyCode.K;
    public static KeyCode LeftLookKey = KeyCode.J;
    public static KeyCode RightLookKey = KeyCode.L;
    public static float VerticalLookKeySensitivity = 1f;
    public static float HorizontalLookKeySensitivity = 0.5f;
    public static float Volume = 0.5f;
    public static bool Fullscreen = true;
    public static int Quality = 1;
    #endregion

    private void OnDestroy()
    {
        SaveCurrentConfig();
    }
    private void Start()
    {
        if (GameObject.FindGameObjectsWithTag("DataHandler").Length > 1)
            Destroy(gameObject);
        LoadCurrentConfig();
        DontDestroyOnLoad(gameObject);
        SceneManager.activeSceneChanged += (a1, a2) => { SaveCurrentConfig(); ApplyCurrentConfig(); };
        ApplyCurrentConfig();
    }
    public void SaveCurrentConfig()
    {
        Dictionary<string, string> temp = new Dictionary<string, string>()
        {
            { "ForwardKey", ForwardKey.ToString() },
            { "BackwardsKey", BackwardsKey.ToString() },
            { "LeftKey", LeftKey.ToString() },
            { "RightKey", RightKey.ToString() },
            { "SlideKey", SlideKey.ToString() },
            { "JumpKey", JumpKey.ToString() },
            { "RespawnKey", RespawnKey.ToString() },
            { "ShootKey", ShootKey.ToString() },
            { "ActivateKey", ActivateKey.ToString() },
            { "UseChangeKey", UseChangeKey.ToString() },
            { "ChangeKey", ChangeKey.ToString() },
            { "VerticalMouseSensitivity", VerticalMouseSensitivity.ToString() },
            { "HorizontalMouseSensitivity", HorizontalMouseSensitivity.ToString() },
            { "UseFixedDistanceLookKeys", UseFixedDistanceLookKeys.ToString() },
            { "UpLookKey", UpLookKey.ToString() },
            { "DownLookKey", DownLookKey.ToString() },
            { "LeftLookKey", LeftLookKey.ToString() },
            { "RightLookKey", RightLookKey.ToString() },
            { "VerticalLookKeySensitivity", VerticalLookKeySensitivity.ToString() },
            { "HorizontalLookKeySensitivity", HorizontalLookKeySensitivity.ToString() },
            { "Volume", Volume.ToString() },
            { "Fullscreen", Fullscreen.ToString() },
            { "Quality", Quality.ToString() },
        };

        SaveConfig(temp);
    }
    public void ApplyCurrentConfig()
    {
        GameObject.FindGameObjectsWithTag("Player").ToList().ForEach(o => {
            o.GetComponent<PlayerInputScript>().ForwardKey = ForwardKey;
            o.GetComponent<PlayerInputScript>().BackwardsKey = BackwardsKey;
            o.GetComponent<PlayerInputScript>().LeftKey = LeftKey;
            o.GetComponent<PlayerInputScript>().RightKey = RightKey;
            o.GetComponent<PlayerInputScript>().SlideKey = SlideKey;
            o.GetComponent<PlayerInputScript>().JumpKey = JumpKey;
            o.GetComponent<PlayerInputScript>().RespawnKey = RespawnKey;
            o.GetComponent<PlayerInputScript>().ShootKey = ShootKey;
            o.GetComponent<PlayerInputScript>().ActivateKey = ActivateKey;
            o.GetComponent<PlayerInputScript>().UseChangeKey = UseChangeKey;
            o.GetComponent<PlayerInputScript>().ChangeKey = ChangeKey;
            o.GetComponent<PlayerInputScript>().VerticalMouseSensitivity = VerticalMouseSensitivity;
            o.GetComponent<PlayerInputScript>().HorizontalMouseSensitivity = HorizontalMouseSensitivity;
            o.GetComponent<PlayerInputScript>().UseFixedDistanceLookKeys = UseFixedDistanceLookKeys;
            o.GetComponent<PlayerInputScript>().UpLookKey = UpLookKey;
            o.GetComponent<PlayerInputScript>().DownLookKey = DownLookKey;
            o.GetComponent<PlayerInputScript>().LeftLookKey = LeftLookKey;
            o.GetComponent<PlayerInputScript>().RightLookKey = RightLookKey;
            o.GetComponent<PlayerInputScript>().VerticalLookKeySensitivity = VerticalLookKeySensitivity;
            o.GetComponent<PlayerInputScript>().HorizontalLookKeySensitivity = HorizontalLookKeySensitivity;
        });
        GameObject.FindGameObjectsWithTag("Music").ToList().ForEach(o =>
        {
            o.GetComponent<MusicScript>().Volume = Volume;
        });
        Screen.fullScreen = Fullscreen;
        QualitySettings.SetQualityLevel(Quality);
    }
    public void LoadCurrentConfig()
    {
        Dictionary<string, string> temp = ReadConfig();
        if (temp == null)
        {
            Debug.Log("Couldn't load it");
            return;
        }
        temp.ToList().ForEach(kv =>
        {
            switch (kv.Key) {
                case "ForwardKey":
                    ForwardKey = KeyCodeParser(kv.Value);
                    break;
                case "BackwardsKey":
                    BackwardsKey = KeyCodeParser(kv.Value);
                    break;
                case "LeftKey":
                    LeftKey = KeyCodeParser(kv.Value);
                    break;
                case "RightKey":
                    RightKey = KeyCodeParser(kv.Value);
                    break;
                case "SlideKey":
                    SlideKey = KeyCodeParser(kv.Value);
                    break;
                case "JumpKey":
                    JumpKey = KeyCodeParser(kv.Value);
                    break;
                case "RespawnKey":
                    RespawnKey = KeyCodeParser(kv.Value);
                    break;
                case "ShootKey":
                    ShootKey = KeyCodeParser(kv.Value);
                    break;
                case "ActivateKey":
                    ActivateKey = KeyCodeParser(kv.Value);
                    break;
                case "UseChangeKey":
                    UseChangeKey = bool.Parse(kv.Value);
                    break;
                case "ChangeKey":
                    ChangeKey = KeyCodeParser(kv.Value);
                    break;
                case "VerticalMouseSensitivity":
                    VerticalMouseSensitivity = float.Parse(kv.Value);
                    break;
                case "HorizontalMouseSensitivity":
                    HorizontalMouseSensitivity = float.Parse(kv.Value);
                    break;
                case "UseFixedDistanceLookKeys":
                    UseFixedDistanceLookKeys = bool.Parse(kv.Value);
                    break;
                case "UpLookKey":
                    UpLookKey = KeyCodeParser(kv.Value);
                    break;
                case "DownLookKey":
                    DownLookKey = KeyCodeParser(kv.Value);
                    break;
                case "LeftLookKey":
                    LeftLookKey = KeyCodeParser(kv.Value);
                    break;
                case "RightLookKey":
                    RightLookKey = KeyCodeParser(kv.Value);
                    break;
                case "VerticalLookKeySensitivity":
                    VerticalLookKeySensitivity = float.Parse(kv.Value);
                    break;
                case "HorizontalLookKeySensitivity":
                    HorizontalLookKeySensitivity = float.Parse(kv.Value);
                    break;
                case "Volume":
                    Volume = Convert.ToSingle(kv.Value);
                    break;
                case "Fullscreen":
                    Fullscreen = Convert.ToBoolean(kv.Value);
                    break;
                case "Quality":
                    Quality = int.Parse(kv.Value);
                    break;
            }
        });
    }

    KeyCode KeyCodeParser(string str)
    {
        return (KeyCode)Enum.Parse(typeof(KeyCode), str);
    }

    string[] ConvertToString(Dictionary<string, string> obj)
    {
        List<string> converted = new List<string>();
        obj.ToList().ForEach(kv => converted.Add($"{kv.Key}:{kv.Value}"));
        return converted.ToArray();
    }

    Dictionary<string, string> ConvertFromString(string[] obj)
    {
        Dictionary<string, string> converted = new Dictionary<string, string>();
        obj.Select(s => s.Split(':')).ToList().ForEach(s => converted.Add(s[0], s[1]));
        return converted;
    }

    Dictionary<string, string> ReadConfig()
    {
        try
        {
            return ConvertFromString(File.ReadAllLines(dataPath + fileName));
        }
        catch
        {
            return null;
        }
    }
    void SaveConfig(Dictionary<string, string> config)
    {
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);
        File.WriteAllLines(dataPath + fileName, ConvertToString(config));
    }
}
