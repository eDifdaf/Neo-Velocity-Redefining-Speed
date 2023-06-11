using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonMapping : MonoBehaviour
{
    /*
    [SerializeField] KeyCode ForwardKey = KeyCode.W;
    [SerializeField] KeyCode BackwardsKey = KeyCode.S;
    [SerializeField] KeyCode LeftKey = KeyCode.A;
    [SerializeField] KeyCode RightKey = KeyCode.D;
    [SerializeField] KeyCode SlideKey = KeyCode.LeftShift;
    [SerializeField] KeyCode JumpKey = KeyCode.Space;
    [SerializeField] KeyCode RespawnKey = KeyCode.R;
    [SerializeField] KeyCode ShootKey = KeyCode.Mouse0;
    [SerializeField] KeyCode ActivateKey = KeyCode.Mouse1;
    /// <summary>
    /// <p>false -> Tool changes by scrolling</p>
    /// <br></br>
    /// <p>true  -> Tool changes with the ChangeKey</p>
    /// </summary>
    [SerializeField] bool UseChangeKey = true;
    [SerializeField] KeyCode ChangeKey = KeyCode.Mouse2;
    [SerializeField] float VerticalMouseSensitivity = 4f;
    [SerializeField] float HorizontalMouseSensitivity = 7f;
    /// <summary>
    /// <p>false -> Only Mouse is used to move the Camera</p>
    /// <br></br>
    /// <p>true  -> Additional to Mouse Movement, the 4 Look Keys are used to move the camera</p>
    /// </summary>
    [SerializeField] bool UseFixedDistanceLookKeys = true;
    [SerializeField] KeyCode UpLookKey = KeyCode.I;
    [SerializeField] KeyCode DownLookKey = KeyCode.K;
    [SerializeField] KeyCode LeftLookKey = KeyCode.J;
    [SerializeField] KeyCode RightLookKey = KeyCode.L;
    [SerializeField] float VerticalLookKeySensitivity = 1f;
    [SerializeField] float HorizontalLookKeySensitivity = 0.5f;
    */
    [SerializeField] public GameObject PlayerPrefab;
    PlayerInputScript inputScript;
    bool awaitingButton;
    string ButtonToChange;
    
    public void Start()
    {
        awaitingButton = false;
        inputScript = PlayerPrefab.GetComponent<PlayerInputScript>();
    }
    public void Update()
    {
        if (!awaitingButton)
            return;

        Event temp = Event.current;
        if (temp.isKey){
            if (temp.type == EventType.KeyDown)
            {
                awaitingButton = false;
                Debug.Log(temp.keyCode);
                ChangeButton(temp.keyCode);
            }
        }

        if (Input.anyKeyDown && awaitingButton)
        {
            awaitingButton = false;
            string keyPressed = Input.inputString;
            Debug.Log(keyPressed);
            
        }
    }
    public void SheduleButtonChange(string name)
    {
        awaitingButton = true;
        ButtonToChange = name;
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
    }
    public void GetNewButton()
    {
        
    }
}
