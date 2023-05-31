using UnityEngine;
using UnityEngine.UI;

public class Time_measure : MonoBehaviour
{
    public string spawnTag = "Spawn";
    public string finishTag = "Finish";

    private bool timing;
    private float startTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(spawnTag))
        {
            timing = true;
            startTime = Time.time;
        }

        if (other.CompareTag(finishTag) && timing)
        {
            timing = false;
            float elapsedTime = Time.time - startTime;
            DisplayTime(elapsedTime);
        }
    }

    private void DisplayTime(float time)
    {
        // Assuming you have a Text component attached to the same GameObject
        Text textUI = GetComponent<Text>();

        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time % 1) * 1000);

        string timeText = minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + milliseconds.ToString("000");
        textUI.text = "Time: " + timeText;

        Debug.Log("Elapsed Time: " + timeText);
    }
}
