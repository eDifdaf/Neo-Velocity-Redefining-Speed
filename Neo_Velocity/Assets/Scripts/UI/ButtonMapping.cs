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
    PlayerInputScript inputScript;
    bool awaitingButton;
    string ButtonToChange;
    ButtonMappingInfoHolder prevInfo;
    string prevText;
    
    public void Start()
    {
        awaitingButton = false;
        inputScript = PlayerPrefab.GetComponent<PlayerInputScript>();

        gameObject.GetComponentsInChildren<ButtonMappingInfoHolder>(true).Where(s => s.button != null).ToList().ForEach(s =>
        {
            KeyCode key = KeyCode.None;
            switch (s.text)
            {
                case "Forward Key":
                    key = inputScript.ForwardKey;
                    break;
                case "Backwards Key":
                    key = inputScript.BackwardsKey;
                    break;
                case "Left Key":
                    key = inputScript.LeftKey;
                    break;
                case "Right Key":
                    key = inputScript.RightKey;
                    break;
                case "Look up Key":
                    key = inputScript.UpLookKey;
                    break;
                case "Look down Key":
                    key = inputScript.DownLookKey;
                    break;
                case "Look left Key":
                    key = inputScript.LeftLookKey;
                    break;
                case "Look right Key":
                    key = inputScript.RightLookKey;
                    break;
                case "Slide Key":
                    key = inputScript.SlideKey;
                    break;
                case "Jump Key":
                    key = inputScript.JumpKey;
                    break;
                case "Respawn Key":
                    key = inputScript.RespawnKey;
                    break;
                case "Shoot Key":
                    key = inputScript.ShootKey;
                    break;
                case "Activate Key":
                    key = inputScript.ActivateKey;
                    break;
                case "Tool Change Key":
                    key = inputScript.ChangeKey;
                    break;
            }
            s.button.GetComponentInChildren<TMP_Text>().text = key.ToString();
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
                inputScript.ForwardKey = newKey;
                break;
            case "Backwards Key":
                inputScript.BackwardsKey = newKey;
                break;
            case "Left Key":
                inputScript.LeftKey = newKey;
                break;
            case "Right Key":
                inputScript.RightKey = newKey;
                break;
            case "Look up Key":
                inputScript.UpLookKey = newKey;
                break;
            case "Look down Key":
                inputScript.DownLookKey = newKey;
                break;
            case "Look left Key":
                inputScript.LeftLookKey = newKey;
                break;
            case "Look right Key":
                inputScript.RightLookKey = newKey;
                break;
            case "Slide Key":
                inputScript.SlideKey = newKey;
                break;
            case "Jump Key":
                inputScript.JumpKey = newKey;
                break;
            case "Respawn Key":
                inputScript.RespawnKey = newKey;
                break;
            case "Shoot Key":
                inputScript.ShootKey = newKey;
                break;
            case "Activate Key":
                inputScript.ActivateKey = newKey;
                break;
            case "Tool Change Key":
                inputScript.ChangeKey = newKey;
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
                inputScript.UseChangeKey = info.checkBox;
                break;
            case "Use Look Keys":
                inputScript.UseFixedDistanceLookKeys = info.checkBox;
                break;
        }
    }

    public void ChangeFloat(ButtonMappingInfoHolder info)
    {
        switch (info.text)
        {
            case "Horizontal Mouse Sensitivity":
                if (info.IsSlider)
                    inputScript.HorizontalMouseSensitivity = info.slider.value;
                else
                    inputScript.HorizontalMouseSensitivity = float.Parse(info.inputTextField.text);
                break;
            case "Vertical Mouse Sensitivity":
                if (info.IsSlider)
                    inputScript.VerticalMouseSensitivity = info.slider.value;
                else
                    inputScript.VerticalMouseSensitivity = float.Parse(info.inputTextField.text);
                break;
            case "Horizontal Look Key Sensitivity":
                if (info.IsSlider)
                    inputScript.HorizontalLookKeySensitivity = info.slider.value;
                else
                    inputScript.HorizontalLookKeySensitivity = float.Parse(info.inputTextField.text);
                break;
            case "Vertical Look Key Sensitivity":
                if (info.IsSlider)
                    inputScript.VerticalLookKeySensitivity = info.slider.value;
                else
                    inputScript.VerticalLookKeySensitivity = float.Parse(info.inputTextField.text);
                break;
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Settings");
    }
}
