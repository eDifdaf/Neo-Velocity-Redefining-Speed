using UnityEngine;
using TMPro;

public class TimeMeasure : MonoBehaviour
{
    
    public string spawnTag = "Spawn";
    public string finishTag = "Finish";
    public string timeTag = "Time";

    private bool timing;
    private float startTime;
    private TMP_Text timeText;
    public float? TimeToFinish;

    public void Start() {
        TimeToFinish = null;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(spawnTag)) {
            
            if (timing) {
                timing = false;
                float elapsedTime = Time.time - startTime;
                DisplayTime(elapsedTime);
                //Debug.Log("Player touched the finish object");
            }
            else {
                ResetTimer();
                //Debug.Log("Player entered the spawn area");
            }
        }
        else if (other.CompareTag(finishTag) && timing)
        {
            timing = false;
            float elapsedTime = Time.time - startTime;
            TimeToFinish = elapsedTime;
            DisplayTime(elapsedTime);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(spawnTag))
        {
            timing = true;
            startTime = Time.time;
            //Debug.Log("Player left the spawn area");
        }
    }

    private void Update()
    {
        if (timing)
        {
            float elapsedTime = Time.time - startTime;
            DisplayTime(elapsedTime);
        }
    }

    private void DisplayTime(float elapsedTime)
    {
        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);
        int milliseconds = (int)((elapsedTime * 1000f) % 1000f);

        string timeString = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);

        if (timeText != null)
        {
            timeText.text = timeString;
        }
        else
        {
            Debug.LogError("Time Text component not found with the specified tag: " + timeTag);
        }
    }

    private void ResetTimer()
    {
        timing = false;
        startTime = 0f;

        if (timeText != null)
        {
            DisplayTime(0f);
        }
        else
        {
            Debug.LogError("Time Text component not found with the specified tag: " + timeTag);
        }
    }

    public void InitializeTimer()
    {
        timeText = GameObject.FindGameObjectWithTag(timeTag)?.GetComponent<TMP_Text>();

        if (timeText == null)
        {
            Debug.LogError("Time Text component not found with the specified tag: " + timeTag);
        }
    }
}
