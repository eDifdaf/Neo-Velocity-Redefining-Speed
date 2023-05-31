using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayerInputScript : AInputScript
{
    [SerializeField] KeyCode SlideKey = KeyCode.LeftShift;
    [SerializeField] KeyCode JumpKey = KeyCode.Space;
    [SerializeField] KeyCode RespawnKey = KeyCode.R;

    [SerializeField] string ReplayFolderLocation = "Replays\\";
    StreamWriter replayWriter;
    string WriterLocation;

    public bool SaveReplay;

    float MouseXBuffer;
    float MouseYBuffer;
    float MouseX;
    float MouseY;
    
    bool MouseUpdateFlag; // true: unevaled, false: evaled

    bool RespawnPressed;
    bool ShootPressed;

    bool AlreadyFinished;
    float TimeToFinish;

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
    public override Dictionary<string, float> GetInput()
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
        StringInputs += inputs["Mouse X"].ToString(CultureInfo.InvariantCulture) + ",";
        StringInputs += inputs["Mouse Y"].ToString(CultureInfo.InvariantCulture) + ",";

        inputs.Add("Vertical", Input.GetAxisRaw("Vertical"));
        inputs.Add("Horizontal", Input.GetAxisRaw("Horizontal"));
        StringInputs += inputs["Vertical"].ToString(CultureInfo.InvariantCulture) + ",";
        StringInputs += inputs["Horizontal"].ToString(CultureInfo.InvariantCulture) + ",";

        if (Input.GetMouseButton(0))
        {
            if (ShootPressed)
                inputs.Add("Shoot", 0);
            else
            {
                ShootPressed = true;
                inputs.Add("Shoot", 1);
            }
        }
        else
        {
            inputs.Add("Shoot", 0);
            ShootPressed = false;
        }
        StringInputs += inputs["Shoot"];

        if (SaveReplay)
            replayWriter.WriteLine(StringInputs);

        return inputs;
    }
    public override Vector2 GetMouseInput()
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
    public override void ResetInputs()
    {
        AlreadyFinished = false;
        if (replayWriter != null)
            ResetWriter();
        if (!Directory.Exists(ReplayFolderLocation))
            Directory.CreateDirectory(ReplayFolderLocation);
        WriterLocation = ReplayFolderLocation + "Replay_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + ".replay";
        replayWriter = new StreamWriter(WriterLocation);
        replayWriter.WriteLine("Format: 31_5_2023");
        replayWriter.WriteLine("Not Finished");
    }
    public void Finished(float time)
    {
        if (AlreadyFinished)
        {
            TimeToFinish = Math.Min(TimeToFinish, time);
            return;
        }
        TimeToFinish = time;
        AlreadyFinished = true;
    }
    void ResetWriter()
    {
        replayWriter.Close();
        if (AlreadyFinished)
        {
            string[] arrLine = File.ReadAllLines(WriterLocation);
            arrLine[1] = TimeToFinish.ToString(CultureInfo.InvariantCulture);
            File.WriteAllLines(WriterLocation, arrLine);
        }
    }

    private void Start()
    {
        MouseX = 0;
        MouseY = 0;
        MouseUpdateFlag = true;
        MouseXBuffer = 0;
        MouseYBuffer = 0;
        RespawnPressed = false;
        ShootPressed = false;
    }

    private void OnDestroy()
    {
        if (replayWriter != null)
            ResetWriter();
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
