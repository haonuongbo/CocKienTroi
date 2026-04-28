using TMPro;
using UnityEngine;

public class LapManager2D : MonoBehaviour
{
    public int maxLap = 3;
    public GameObject finishCanvas;
    public GameObject winCanvasData; // WinCnCanvasData
    
    [Header("---- WIN CANVAS UI ----")]
    public TextMeshProUGUI winLapText; // Kéo thẻ RaceLapText ở WinCanvas vào đây

    [Header("---- HUD UI ----")]
    public GameObject hudToHide; // Kéo Joystick HUD vào đây (tùy chọn)

    public bool hideFinishCanvasOnStart = true;

    public int currentLap = 0;
    public TextMeshProUGUI lapText;

    private bool hasFinished = false;
    private float nextAllowedCountTime;
    private MonoBehaviour[] winCanvasBehaviours;
    private const float CountCooldownSeconds = 1.5f;

    void Start()
    {
        if (hudToHide == null) 
        {
            // Auto find hud
            GameObject findHud = GameObject.Find("JoyStick HUD(Clone)");
            if (findHud != null) hudToHide = findHud;
            else 
            {
                 findHud = GameObject.Find("JoyStick HUD");
                 if (findHud != null) hudToHide = findHud;
            }
        }

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

            // Ẩn Joystick HUD (Chỉ ẩn phần Canvas để giữ lại EventSystem)
            if (hudToHide != null)
            {
                Transform canvasChild = hudToHide.transform.Find("HUD_Canvas");
                if (canvasChild != null)
                    canvasChild.gameObject.SetActive(false);
                else
                    hudToHide.SetActive(false);
            }

            // Dừng game manager đếm giờ và lấy giờ đẩy lên Win Canvas
            if (GameManager.Instance != null)
                GameManager.Instance.StopRaceTime();

            // CHỜ 5 GIÂY RỒI MỚI XUẤT HIỆN BẢNG WIN VÀ VIDEO
            StartCoroutine(ShowWinDelay());
        }

        nextAllowedCountTime = Time.time + CountCooldownSeconds;
        return true;
    }

    private System.Collections.IEnumerator ShowWinDelay()
    {
        // ===================================
        // 1. TẠO & CHẠY ẢNH ĐỘNG PHÁO BÔNG TRONG SUỐT (TRONG LÚC CHỜ 5 GIÂY)
        // ===================================
        GameObject gifObj = new GameObject("FireworksGif");
        var canvas = gifObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        var rawImage = gifObj.AddComponent<UnityEngine.UI.RawImage>();
        rawImage.raycastTarget = false;
        
        // Full screen
        var rect = rawImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        StartCoroutine(PlayFrames(rawImage));

        // Bật âm thanh Về đích và giảm nhạc nền đua
        AudioClip winAudio = Resources.Load<AudioClip>("Audio/finish_sound");
        if (winAudio != null)
        {
            var bgms = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach(var a in bgms) 
            {
                if (a.loop && a.gameObject.name != "WinSound") a.volume = 0.1f;
            }
            
            var audioObj = new GameObject("WinSound");
            var src = audioObj.AddComponent<AudioSource>();
            src.clip = winAudio;
            src.spatialBlend = 0;
            src.Play();
        }

        // ===================================
        // 2. CHỜ 5 GIÂY CHO XE CHẠY THÊM
        // ===================================
        yield return new WaitForSeconds(2.96f);

        // (Tùy chọn) Ẩn video pháo bông sau khi kết thúc 5s
        if (gifObj != null) Destroy(gifObj);

        // ===================================
        // 3. SHOW BẢNG WIN UI THEO KẾ HOẠCH
        // ===================================
        if (finishCanvas != null)
        {
            finishCanvas.SetActive(true);
        }

        // Cập nhật thẻ chữ Lap ở Win Canvas
        if (winLapText == null)
        {
            GameObject lapTextObj = GameObject.Find("RaceLapText");
            if (lapTextObj != null) winLapText = lapTextObj.GetComponent<TextMeshProUGUI>();
        }

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

    private System.Collections.IEnumerator PlayFrames(UnityEngine.UI.RawImage img)
    {
        var frames = new System.Collections.Generic.List<Texture2D>();
        for (int i = 0; i < 74; i++)
        {
            var tex = Resources.Load<Texture2D>("FireworksFrames/frame_" + i.ToString("D3"));
            if (tex != null) frames.Add(tex);
        }

        if (frames.Count == 0) yield break;

        int index = 0;
        // FPS của ảnh động GIF gốc (khoảng 30-33 fps)
        WaitForSeconds waitScale = new WaitForSeconds(0.04f); 

        while (img != null)
        {
            img.texture = frames[index];
            index = (index + 1) % frames.Count;
            yield return waitScale;
        }
    }

    void UpdateLapText()
    {
        if (lapText != null)
            lapText.text = "Lap " + Mathf.Min(currentLap, maxLap) + "/" + maxLap;
    }
}