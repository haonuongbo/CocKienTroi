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

    private bool hasFinished = false;
    private float nextAllowedCountTime;
    private MonoBehaviour[] winCanvasBehaviours;
    private const float CountCooldownSeconds = 1.5f;

    void Start()
    {
        if (winCanvasData != null)
            winCanvasBehaviours = winCanvasData.GetComponentsInChildren<MonoBehaviour>(true);

        UpdateLapText();

        if (hideFinishCanvasOnStart && finishCanvas != null)
            finishCanvas.SetActive(false);

        if (winCanvasData != null)
            winCanvasData.SetActive(true); // đảm bảo đang hoạt động lúc đầu
    }

    public void CountLap()
    {
        if (hasFinished || Time.time < nextAllowedCountTime) return;

        currentLap++;
        UpdateLapText();

        if (currentLap >= maxLap)
        {
            hasFinished = true;

            // show win UI
            if (finishCanvas != null)
                finishCanvas.SetActive(true);

            // stop updating WinCnCanvasData
            if (winCanvasBehaviours != null)
            {
                foreach (var s in winCanvasBehaviours)
                    s.enabled = false;
            }
        }

        nextAllowedCountTime = Time.time + CountCooldownSeconds;
    }

    void UpdateLapText()
    {
        if (lapText != null)
            lapText.text = "Lap " + Mathf.Min(currentLap, maxLap) + "/" + maxLap;
    }
}