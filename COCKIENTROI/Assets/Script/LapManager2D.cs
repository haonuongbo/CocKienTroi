using TMPro;
using UnityEngine;

public class LapManager2D : MonoBehaviour
{
    public int maxLap = 3;
    public GameObject finishCanvas;
    public GameObject winCanvasData; // WinCnCanvasData
    
    [Header("---- WIN CANVAS UI ----")]
    public TextMeshProUGUI winLapText; // Kéo thẻ RaceLapText ở WinCanvas vào đây

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

        if (lapText == null)
            lapText = FindLapText();

        if (winLapText == null)
            winLapText = FindWinLapText();

        UpdateLapText();

        if (hideFinishCanvasOnStart && finishCanvas != null)
            finishCanvas.SetActive(false);

        if (winCanvasData != null)
            winCanvasData.SetActive(true); // đảm bảo đang hoạt động lúc đầu
    }

    private TextMeshProUGUI FindLapText()
    {
        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name == "Txt_LapCount")
                return texts[i];
        }

        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].text != null && texts[i].text.Contains("Lap"))
                return texts[i];
        }

        return null;
    }

    private TextMeshProUGUI FindWinLapText()
    {
        if (finishCanvas != null)
        {
            TextMeshProUGUI[] texts = finishCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].name == "RaceLapText")
                    return texts[i];
            }
        }

        return null;
    }

    public bool CountLap()
    {
        if (hasFinished) return false;
        
        if (Time.time < nextAllowedCountTime) return false;

        currentLap++;
        UpdateLapText();

        if (currentLap >= maxLap)
        {
            hasFinished = true;

            // show win UI
            if (finishCanvas != null)
                finishCanvas.SetActive(true);

            // Dừng game manager đếm giờ và lấy giờ đẩy lên Win Canvas
            if (GameManager.Instance != null)
                GameManager.Instance.StopRaceTime();

            // Cập nhật thẻ chữ Lap ở Win Canvas
            if (winLapText != null)
            {
                winLapText.text = "LAPS: " + maxLap.ToString() + " / " + maxLap.ToString();
            }

            // stop updating WinCnCanvasData
            if (winCanvasBehaviours != null)
            {
                foreach (var s in winCanvasBehaviours)
                    s.enabled = false;
            }
        }

        nextAllowedCountTime = Time.time + CountCooldownSeconds;
        return true;
    }

    void UpdateLapText()
    {
        if (lapText != null)
            lapText.text = "Lap " + Mathf.Min(currentLap, maxLap) + "/" + maxLap;
    }
}