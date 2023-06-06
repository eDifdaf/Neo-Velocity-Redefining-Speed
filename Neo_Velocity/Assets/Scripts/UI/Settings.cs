using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Windows;

public class Settings : AInputScript
{
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
}
