using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonMapping : MonoBehaviour
{
    [SerializeField] public GameObject PlayerPrefab;
    PlayerInputScript inputScript;
    bool awaitingButton;
    string ButtonToChange;
    ButtonMappingInfoHolder prevInfo;
    
    public void Start()
    {
        awaitingButton = false;
        inputScript = PlayerPrefab.GetComponent<PlayerInputScript>();
    }
    public void Update()
    {
        if (!awaitingButton)
            return;

        // This is for getting the pressed Key
        // If it doesn't work, please find a different one
        // Then pass it to -> ChangeButton(KeyCode newKey)
        // You should be able to tell from the logs if it works or not
        Event temp = Event.current;
        if (temp.isKey){
            if (temp.type == EventType.KeyDown)
            {
                awaitingButton = false;
                Debug.Log(temp.keyCode);
                ChangeButton(temp.keyCode);
            }
        }
    }
    public void SheduleButtonChange(ButtonMappingInfoHolder info)
    {
        if (awaitingButton)
        {
            prevInfo.text_field.text = prevInfo.text;
        }
        awaitingButton = true;
        ButtonToChange = info.text;
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

        prevInfo.text_field.text = prevInfo.text;
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
}
