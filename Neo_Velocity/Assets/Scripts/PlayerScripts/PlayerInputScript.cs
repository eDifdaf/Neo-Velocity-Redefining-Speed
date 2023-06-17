using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerInputScript : AInputScript
{
    [SerializeField] public KeyCode ForwardKey = KeyCode.W;
    [SerializeField] public KeyCode BackwardsKey = KeyCode.S;
    [SerializeField] public KeyCode LeftKey = KeyCode.A;
    [SerializeField] public KeyCode RightKey = KeyCode.D;
    [SerializeField] public KeyCode SlideKey = KeyCode.LeftControl;
    [SerializeField] public KeyCode JumpKey = KeyCode.Space;
    [SerializeField] public KeyCode RespawnKey = KeyCode.R;
    [SerializeField] public KeyCode ShootKey = KeyCode.Mouse0;
    [SerializeField] public KeyCode ActivateKey = KeyCode.Mouse1;
    /// <summary>
    /// <p>false -> Tool changes by scrolling</p>
    /// <br></br>
    /// <p>true  -> Tool changes with the ChangeKey</p>
    /// </summary>
    [SerializeField] public bool UseChangeKey = false;
    [SerializeField] public KeyCode ChangeKey = KeyCode.Mouse2;
    [SerializeField] public float VerticalMouseSensitivity = 4f;
    [SerializeField] public float HorizontalMouseSensitivity = 7f;
    /// <summary>
    /// <p>false -> Only Mouse is used to move the Camera</p>
    /// <br></br>
    /// <p>true  -> Additional to Mouse Movement, the 4 Look Keys are used to move the camera</p>
    /// </summary>
    [SerializeField] public bool UseFixedDistanceLookKeys = true;
    [SerializeField] public KeyCode UpLookKey = KeyCode.I;
    [SerializeField] public KeyCode DownLookKey = KeyCode.K;
    [SerializeField] public KeyCode LeftLookKey = KeyCode.J;
    [SerializeField] public KeyCode RightLookKey = KeyCode.L;
    [SerializeField] public float VerticalLookKeySensitivity = 1f;
    [SerializeField] public float HorizontalLookKeySensitivity = 0.5f;


    [SerializeField] string ReplayFolderLocation = "Replays\\";

    StreamWriter replayWriter;
    string WriterLocation;

    public bool SaveReplay;

    float MouseXBuffer;
    float MouseYBuffer;
    float MouseX;
    float MouseY;
    float ScrollBuffer;
    float Scroll;
    
    bool MouseUpdateFlag; // true: unevaled, false: evaled

    bool RespawnPressed;
    bool ShootPressed;
    bool ActivatePressed;

    bool AlreadyFinished;
    float TimeToFinish;
    bool notRenamed;

    /// <summary>
    /// !!!not accurate, to lazy to change!!!!
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

        if (UseFixedDistanceLookKeys)
        {
            int ul = Input.GetKey(UpLookKey) ? 1 : 0;
            int dl = Input.GetKey(DownLookKey) ? 1 : 0;
            int ll = Input.GetKey(LeftLookKey) ? 1 : 0;
            int rl = Input.GetKey(RightLookKey) ? 1 : 0;
            inputs.Add("Mouse X", MouseXBuffer + (rl - ll) * HorizontalLookKeySensitivity);
            inputs.Add("Mouse Y", MouseYBuffer + (ul - dl) * VerticalLookKeySensitivity);
        }
        else
        {
            inputs.Add("Mouse X", MouseXBuffer);
            inputs.Add("Mouse Y", MouseYBuffer);
        }
        MouseXBuffer = 0;
        MouseYBuffer = 0;
        StringInputs += inputs["Mouse X"].ToString(CultureInfo.InvariantCulture) + ",";
        StringInputs += inputs["Mouse Y"].ToString(CultureInfo.InvariantCulture) + ",";

        int fwd = Input.GetKey(ForwardKey) ? 1 : 0;
        int bwd = Input.GetKey(BackwardsKey) ? 1 : 0;
        int lst = Input.GetKey(RightKey) ? 1 : 0;
        int rst = Input.GetKey(LeftKey) ? 1 : 0;
        inputs.Add("Vertical", fwd - bwd);
        inputs.Add("Horizontal", lst - rst);
        StringInputs += inputs["Vertical"].ToString(CultureInfo.InvariantCulture) + ",";
        StringInputs += inputs["Horizontal"].ToString(CultureInfo.InvariantCulture) + ",";

        if (Input.GetKey(ShootKey))
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
        StringInputs += inputs["Shoot"] + ",";

        if (Input.GetKey(ActivateKey))
        {
            if (ActivatePressed)
                inputs.Add("Activate", 0);
            else
            {
                ActivatePressed = true;
                inputs.Add("Activate", 1);
            }
        }
        else
        {
            inputs.Add("Activate", 0);
            ActivatePressed = false;
        }
        StringInputs += inputs["Activate"] + ",";

        if (UseChangeKey)
        {
            if (Input.GetKey(ChangeKey))
                inputs.Add("Change", 1);
            else
                inputs.Add("Change", 0);
        }
        else
        {
            if (ScrollBuffer != 0)
                inputs.Add("Change", 1);
            else
                inputs.Add("Change", 0);
        }
        StringInputs += inputs["Change"];
        ScrollBuffer = 0;

        if (SaveReplay)
            replayWriter.WriteLine(StringInputs);

        return inputs;
    }
    public override Vector2 GetMouseInput()
    {
        if (MouseUpdateFlag)
        {
            MouseX = Input.GetAxis("Mouse X") * HorizontalMouseSensitivity;
            MouseY = Input.GetAxis("Mouse Y") * VerticalMouseSensitivity;

            MouseXBuffer += MouseX;
            MouseYBuffer += MouseY;

            MouseUpdateFlag = false;

            Scroll = Input.GetAxis("Mouse ScrollWheel");
            ScrollBuffer += Scroll;
        }
        return new Vector2(MouseX, MouseY);
    }
    public override void ResetInputs()
    {
        if (replayWriter != null)
            ResetWriter();
        AlreadyFinished = false;
        if (!Directory.Exists(ReplayFolderLocation))
            Directory.CreateDirectory(ReplayFolderLocation);
        WriterLocation = ReplayFolderLocation + "Replay_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + ".replay";
        notRenamed = true;
        replayWriter = new StreamWriter(WriterLocation);
        replayWriter.WriteLine("Format: 6_6_2023");
        replayWriter.WriteLine("Not Finished");
        replayWriter.WriteLine(SceneManager.GetActiveScene().name);
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

    public void RenameCurrentFile(string newName)
    {
        replayWriter.Close();
        string newWriterLocation = ReplayFolderLocation + newName + ".replay";
        File.Move(WriterLocation, newWriterLocation);
        WriterLocation = newWriterLocation;
        replayWriter = new StreamWriter(WriterLocation, true);
        notRenamed = false;
    }

    void ResetWriter()
    {
        replayWriter.Close();
        if (AlreadyFinished && !notRenamed)
        {
            string[] arrLine = File.ReadAllLines(WriterLocation);
            arrLine[1] = TimeToFinish.ToString(CultureInfo.InvariantCulture);
            File.WriteAllLines(WriterLocation, arrLine);
            TimeToFinish = 0;
        }
        else
        {
            File.Delete(WriterLocation);
        }
    }

    private void Start()
    {
        Scroll = 0;
        ScrollBuffer = 0;
        MouseX = 0;
        MouseY = 0;
        MouseUpdateFlag = true;
        MouseXBuffer = 0;
        MouseYBuffer = 0;
        RespawnPressed = false;
        ShootPressed = false;
        ActivatePressed = false;
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

            Scroll = Input.GetAxis("Mouse ScrollWheel");
            ScrollBuffer += Scroll;
        }
    }

    private void LateUpdate()
    {
        MouseUpdateFlag = true;
    }
}
