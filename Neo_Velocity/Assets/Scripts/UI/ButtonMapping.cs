using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ButtonMapping : MonoBehaviour
{
    [SerializeField] public GameObject PlayerPrefab;
    bool awaitingButton;
    string ButtonToChange;
    ButtonMappingInfoHolder prevInfo;
    string prevText;
    
    public void Start()
    {
        awaitingButton = false;

        gameObject.GetComponentsInChildren<ButtonMappingInfoHolder>(true).Where(s => s.button != null).ToList().ForEach(s =>
        {
            KeyCode key = KeyCode.None;
            switch (s.text)
            {
                case "Forward Key":
                    key = SaveDataManager.ForwardKey;
                    break;
                case "Backwards Key":
                    key = SaveDataManager.BackwardsKey;
                    break;
                case "Left Key":
                    key = SaveDataManager.LeftKey;
                    break;
                case "Right Key":
                    key = SaveDataManager.RightKey;
                    break;
                case "Look up Key":
                    key = SaveDataManager.UpLookKey;
                    break;
                case "Look down Key":
                    key = SaveDataManager.DownLookKey;
                    break;
                case "Look left Key":
                    key = SaveDataManager.LeftLookKey;
                    break;
                case "Look right Key":
                    key = SaveDataManager.RightLookKey;
                    break;
                case "Slide Key":
                    key = SaveDataManager.SlideKey;
                    break;
                case "Jump Key":
                    key = SaveDataManager.JumpKey;
                    break;
                case "Respawn Key":
                    key = SaveDataManager.RespawnKey;
                    break;
                case "Shoot Key":
                    key = SaveDataManager.ShootKey;
                    break;
                case "Activate Key":
                    key = SaveDataManager.ActivateKey;
                    break;
                case "Tool Change Key":
                    key = SaveDataManager.ChangeKey;
                    break;
            }
            s.button.GetComponentInChildren<TMP_Text>().text = key.ToString();
        });
        gameObject.GetComponentsInChildren<ButtonMappingInfoHolder>(true).Where(s => s.checkBox != null).ToList().ForEach(s =>
        {
            bool value = true;
            switch (s.text)
            {
                case "Use Change Key":
                    value = SaveDataManager.UseChangeKey;
                    break;
                case "Use Look Keys":
                    value = SaveDataManager.UseFixedDistanceLookKeys;
                    break;
            }
            s.checkBox.isOn = value;
        });
        gameObject.GetComponentsInChildren<ButtonMappingInfoHolder>(true).Where(s => s.slider != null).ToList().ForEach(s =>
        {
            float value = 4f;
            switch (s.text)
            {
                case "Horizontal Mouse Sensitivity":
                    value = SaveDataManager.HorizontalMouseSensitivity;
                    break;
                case "Vertical Mouse Sensitivity":
                    value = SaveDataManager.VerticalMouseSensitivity;
                    break;
                case "Horizontal Look Key Sensitivity":
                    value = SaveDataManager.HorizontalLookKeySensitivity;
                    break;
                case "Vertical Look Key Sensitivity":
                    value = SaveDataManager.VerticalLookKeySensitivity;
                    break;
            }
            s.slider.value = value;
            s.inputTextField.text = value.ToString();
        });
    }
    public void Update()
    {
        if (!awaitingButton)
            return;

        foreach (KeyCode key in (KeyCode[])Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                awaitingButton = false;
                ChangeButton(key);
            }
        }
    }

    public void SheduleButtonChange(ButtonMappingInfoHolder info)
    {
        if (awaitingButton)
        {
            prevInfo.text_field.text = prevText;
        }
        awaitingButton = true;
        ButtonToChange = info.text;
        prevText = info.text_field.text;
        info.text_field.text = "Input Key...";

        prevInfo = info;
    }
    public void ChangeButton(KeyCode newKey)
    {
        switch (ButtonToChange)
        {
            case "Forward Key":
                SaveDataManager.ForwardKey = newKey;
                break;
            case "Backwards Key":
                SaveDataManager.BackwardsKey = newKey;
                break;
            case "Left Key":
                SaveDataManager.LeftKey = newKey;
                break;
            case "Right Key":
                SaveDataManager.RightKey = newKey;
                break;
            case "Look up Key":
                SaveDataManager.UpLookKey = newKey;
                break;
            case "Look down Key":
                SaveDataManager.DownLookKey = newKey;
                break;
            case "Look left Key":
                SaveDataManager.LeftLookKey = newKey;
                break;
            case "Look right Key":
                SaveDataManager.RightLookKey = newKey;
                break;
            case "Slide Key":
                SaveDataManager.SlideKey = newKey;
                break;
            case "Jump Key":
                SaveDataManager.JumpKey = newKey;
                break;
            case "Respawn Key":
                SaveDataManager.RespawnKey = newKey;
                break;
            case "Shoot Key":
                SaveDataManager.ShootKey = newKey;
                break;
            case "Activate Key":
                SaveDataManager.ActivateKey = newKey;
                break;
            case "Tool Change Key":
                SaveDataManager.ChangeKey = newKey;
                break;
        }
        prevInfo.button.gameObject.GetComponentInChildren<TMP_Text>().text = newKey.ToString();
        prevInfo.text_field.text = prevText;
    }

    public void ChangeBool(ButtonMappingInfoHolder info)
    {
        switch (info.text)
        {
            case "Use Change Key":
                SaveDataManager.UseChangeKey = info.checkBox;
                break;
            case "Use Look Keys":
                SaveDataManager.UseFixedDistanceLookKeys = info.checkBox;
                break;
        }
    }

    public void ChangeFloat(ButtonMappingInfoHolder info)
    {
        switch (info.text)
        {
            case "Horizontal Mouse Sensitivity":
                if (info.IsSlider)
                {
                    SaveDataManager.HorizontalMouseSensitivity = info.slider.value;
                    info.inputTextField.text = info.slider.value.ToString();
                }
                else
                {
                    SaveDataManager.HorizontalMouseSensitivity = float.Parse(info.inputTextField.text);
                    info.slider.value = float.Parse(info.inputTextField.text);
                }
                break;
            case "Vertical Mouse Sensitivity":
                if (info.IsSlider)
                {
                    SaveDataManager.VerticalMouseSensitivity = info.slider.value;
                    info.inputTextField.text = info.slider.value.ToString();
                }
                else
                {
                    SaveDataManager.VerticalMouseSensitivity = float.Parse(info.inputTextField.text);
                    info.slider.value = float.Parse(info.inputTextField.text);
                }
                break;
            case "Horizontal Look Key Sensitivity":
                if (info.IsSlider)
                {
                    SaveDataManager.HorizontalLookKeySensitivity = info.slider.value;
                    info.inputTextField.text = info.slider.value.ToString();
                }
                else
                {
                    SaveDataManager.HorizontalLookKeySensitivity = float.Parse(info.inputTextField.text);
                    info.slider.value = float.Parse(info.inputTextField.text);
                }
                break;
            case "Vertical Look Key Sensitivity":
                if (info.IsSlider)
                {
                    SaveDataManager.VerticalLookKeySensitivity = info.slider.value;
                    info.inputTextField.text = info.slider.value.ToString();
                }
                else
                {
                    SaveDataManager.VerticalLookKeySensitivity = float.Parse(info.inputTextField.text);
                    info.slider.value = float.Parse(info.inputTextField.text);
                }
                break;

        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Settings");
    }
}
