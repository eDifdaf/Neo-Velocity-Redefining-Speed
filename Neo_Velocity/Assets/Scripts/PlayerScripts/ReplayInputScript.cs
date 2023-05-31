using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

public class ReplayInputScript : AInputScript
{
    [SerializeField] string ReplayFolderLocation = "Replays\\";
    [SerializeField] string ReplayName;

    StreamReader replayReader;

    private Func<Dictionary<string, float>> getInputs;
    public override Dictionary<string, float> GetInput()
    {
        return getInputs();
    }
    public override Vector2 GetMouseInput()
    {
        return Vector2.zero;
    }
    public override void ResetInputs()
    {
        if (replayReader != null)
            replayReader.Close();
        if (!Directory.Exists(ReplayFolderLocation))
            Directory.CreateDirectory(ReplayFolderLocation);
        replayReader = new StreamReader(ReplayFolderLocation + ReplayName);
        switch (replayReader.ReadLine())
        {
            case "Format: 23_5_2023":
                getInputs = () =>
                {
                    Dictionary<string, float> inputs = new Dictionary<string, float>();

                    string[] rawInput = replayReader.ReadLine().Split(',');

                    inputs.Add("Sliding", int.Parse(rawInput[0]));
                    inputs.Add("Jump", int.Parse(rawInput[1]));
                    inputs.Add("Respawn", int.Parse(rawInput[2]));
                    inputs.Add("Mouse X", float.Parse(rawInput[3], CultureInfo.InvariantCulture));
                    inputs.Add("Mouse Y", float.Parse(rawInput[4], CultureInfo.InvariantCulture));
                    inputs.Add("Vertical", float.Parse(rawInput[5], CultureInfo.InvariantCulture));
                    inputs.Add("Horizontal", float.Parse(rawInput[6], CultureInfo.InvariantCulture));

                    return inputs;
                };
                break;
            case "Format: 30_5_2023":
                getInputs = () =>
                {
                    Dictionary<string, float> inputs = new Dictionary<string, float>();

                    string[] rawInput = replayReader.ReadLine().Split(',');

                    inputs.Add("Sliding", int.Parse(rawInput[0]));
                    inputs.Add("Jump", int.Parse(rawInput[1]));
                    inputs.Add("Respawn", int.Parse(rawInput[2]));
                    inputs.Add("Mouse X", float.Parse(rawInput[3], CultureInfo.InvariantCulture));
                    inputs.Add("Mouse Y", float.Parse(rawInput[4], CultureInfo.InvariantCulture));
                    inputs.Add("Vertical", float.Parse(rawInput[5], CultureInfo.InvariantCulture));
                    inputs.Add("Horizontal", float.Parse(rawInput[6], CultureInfo.InvariantCulture));
                    inputs.Add("Shoot", int.Parse(rawInput[7]));

                    return inputs;
                };
                break;
            default:
                Debug.Log("Unrecognized Replay Format");
                break;
        }
    }

    private void Start()
    {
        
    }
    private void OnApplicationQuit()
    {
        if (replayReader != null)
            replayReader.Close();
    }
}
