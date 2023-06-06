using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class UIReplayButtonScript : MonoBehaviour
{
    public string value;
    public Action<string> action;
    public void Execute()
    {
        action(value);
    }
}
