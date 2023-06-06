using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconSwitch_Script : MonoBehaviour
{
    public Image IconC4; // Reference to the Image component for C4 icon
    public Image IconRocketLauncher; // Reference to the Image component for Rocket Launcher icon

    // Start is called before the first frame update
    void Start()
    {
        // Disable C4 icon and enable Rocket Launcher icon initially
        IconC4.enabled = false;
        IconRocketLauncher.enabled = true;
    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null)
            return;
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().SelectedTool == Tools.C4)
        {
            // Enable C4 icon and disable Rocket Launcher icon
            IconC4.enabled = true;
            IconRocketLauncher.enabled = false;
        }
        else
        {
            // Enable Rocket Launcher icon and disable C4 icon
            IconC4.enabled = false;
            IconRocketLauncher.enabled = true;
        }
    }
}
