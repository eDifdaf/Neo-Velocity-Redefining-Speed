using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public abstract class AInputScript : MonoBehaviour
{
    public abstract Dictionary<string, float> GetInput();
    public abstract Vector2 GetMouseInput();
    public abstract void ResetInputs();
}
