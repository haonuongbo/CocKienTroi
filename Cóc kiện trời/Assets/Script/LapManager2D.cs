using UnityEngine;
using UnityEngine.UI;

public class LapManager2D : MonoBehaviour
{
    public int maxLap = 3;
    public int currentLap = 0;
    public Text lapText;

    private bool canCount = true;

    void Start()
    {
        UpdateLapText();
    }

    public void CountLap()
    {
        if (!canCount) return;

        currentLap++;
        if (currentLap > maxLap)
            currentLap = maxLap;

        UpdateLapText();
        canCount = false;
        Invoke(nameof(ResetCount), 1.5f);
    }

    void UpdateLapText()
    {
        lapText.text = "Lap " + currentLap + "/" + maxLap;
    }

    void ResetCount()
    {
        canCount = true;
    }
}