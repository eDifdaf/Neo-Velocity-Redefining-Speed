using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayerInputScript : MonoBehaviour
{
    [SerializeField] KeyCode SlideKey = KeyCode.LeftShift;
    [SerializeField] KeyCode JumpKey = KeyCode.Space;
    [SerializeField] KeyCode RespawnKey = KeyCode.R;

    [SerializeField] string ReplayFolderLocation = "Replays\\";
    StreamWriter replayWriter;

    float MouseXBuffer;
    float MouseYBuffer;
    float MouseX;
    float MouseY;

    bool MouseUpdateFlag; // true: unevaled, false: evaled

    bool RespawnPressed;

    /// <summary>
    /// 
    /// bool = 1/0 -> true/false
    /// <br></br>
    /// <br></br>   Sliding     : bool
    /// <br></br>   Jump        : bool
    /// <br></br>   Mouse X     : float
    /// <br></br>   Mouse Y     : float
    /// <br></br>   Vertical    : float
    /// <br></br>   Horizontal  : float
    /// 
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, float> GetInput()
    {
        Dictionary<string, float> inputs = new Dictionary<string, float>();
        string StringInputs = "";

        if (Input.GetKey(SlideKey))
            inputs.Add("Sliding", 1);
        else
            inputs.Add("Sliding", 0);
        StringInputs += inputs["Sliding"] + ",";

        if (Input.GetKey(JumpKey))
            inputs.Add("Jump", 1);
        else
            inputs.Add("Jump", 0);
        StringInputs += inputs["Jump"] + ",";

        if (Input.GetKey(RespawnKey))
        {
            if (RespawnPressed)
                inputs.Add("Respawn", 0);
            else
            {
                RespawnPressed = true;
                inputs.Add("Respawn", 1);
            }
        }
        else
        {
            inputs.Add("Respawn", 0);
            RespawnPressed = false;
        }
        StringInputs += inputs["Respawn"] + ",";

        inputs.Add("Mouse X", MouseXBuffer);
        inputs.Add("Mouse Y", MouseYBuffer);
        MouseXBuffer = 0;
        MouseYBuffer = 0;
        StringInputs += inputs["Mouse X"] + ",";
        StringInputs += inputs["Mouse Y"] + ",";

        inputs.Add("Vertical", Input.GetAxisRaw("Vertical"));
        inputs.Add("Horizontal", Input.GetAxisRaw("Horizontal"));
        StringInputs += inputs["Vertical"] + ",";
        StringInputs += inputs["Horizontal"];

        replayWriter.WriteLine(StringInputs);

        return inputs;
    }
    public Vector2 GetMouseInput()
    {
        if (MouseUpdateFlag)
        {
            MouseX = Input.GetAxis("Mouse X");
            MouseY = Input.GetAxis("Mouse Y");

            MouseXBuffer += MouseX;
            MouseYBuffer += MouseY;

            MouseUpdateFlag = false;
        }
        return new Vector2(MouseX, MouseY);
    }
    public void ResetInputs()
    {
        if (replayWriter != null)
            replayWriter.Close();
        replayWriter = new StreamWriter(ReplayFolderLocation + "Replay_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + ".replay");
        replayWriter.WriteLine("Format: 23_5_2023");
    }

    private void Start()
    {
        ResetInputs();
        MouseX = 0;
        MouseY = 0;
        MouseUpdateFlag = true;
        MouseXBuffer = 0;
        MouseYBuffer = 0;
        RespawnPressed = false;
    }

    private void OnApplicationQuit()
    {
        replayWriter.Close();
    }

    private void Update()
    {
        if (MouseUpdateFlag)
        {
            MouseX = Input.GetAxis("Mouse X");
            MouseY = Input.GetAxis("Mouse Y");

            MouseXBuffer += MouseX;
            MouseYBuffer += MouseY;

            MouseUpdateFlag = false;
        }
    }

    private void LateUpdate()
    {
        MouseUpdateFlag = true;
    }
}
