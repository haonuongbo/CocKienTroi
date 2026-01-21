using TMPro;
using UnityEngine;

public class UITime : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    private float elapsedTime = 0f;   // replaces raceTime
    private float uiUpdateTimer = 0f;
    private bool isRunning = true;    // control when timer runs

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;

        uiUpdateTimer += Time.deltaTime;
        if (uiUpdateTimer >= 0.05f)
        {
            uiUpdateTimer = 0f;

            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

            if (timeText != null)
            {
                timeText.text = $"{minutes:00}:{seconds:00}:{milliseconds:00}";
            }
        }
    }

    // call this when race starts
    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
    }

    // call this when race stops/finishes
    public void StopTimer()
    {
        isRunning = false;
    }
}
