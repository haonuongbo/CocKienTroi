using TMPro;
using UnityEngine;

public class LapManager2D : MonoBehaviour
{
    public int maxLap = 3;
    public GameObject finishCanvas;
    public GameObject winCanvasData; // WinCnCanvasData

    public bool hideFinishCanvasOnStart = true;

    public int currentLap = 0;
    public TextMeshProUGUI lapText;

    private bool canCount = true;
    private bool hasFinished = false;

    void Start()
    {
        UpdateLapText();

        if (hideFinishCanvasOnStart && finishCanvas != null)
            finishCanvas.SetActive(false);

        if (winCanvasData != null)
            winCanvasData.SetActive(true); // đảm bảo đang hoạt động lúc đầu
    }

    public void CountLap()
    {
        if (!canCount || hasFinished) return;

        currentLap++;

        if (currentLap <= maxLap)
        {
            UpdateLapText();
        }

        if (currentLap > maxLap)
        {
            hasFinished = true;

            // show win UI
            if (finishCanvas != null)
                finishCanvas.SetActive(true);

            // stop updating WinCnCanvasData
            if (winCanvasData != null)
            {
                MonoBehaviour[] scripts = winCanvasData.GetComponentsInChildren<MonoBehaviour>();
                foreach (var s in scripts)
                    s.enabled = false;
            }
        }

        canCount = false;
        Invoke(nameof(ResetCount), 1.5f);
    }

    void UpdateLapText()
    {
        if (lapText != null)
            lapText.text = "Lap " + currentLap + "/" + maxLap;
    }

    void ResetCount()
    {
        canCount = true;
    }
}