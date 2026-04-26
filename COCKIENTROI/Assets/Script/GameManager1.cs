using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("---- CÀI ĐẶT UI ----")]
    public Image numberDisplay;      // Ảnh số 3-2-1
    public GameObject goHolder;      // Cụm chữ GO
    public TMP_Text timeText;        // Kéo cái Txt_Time vào đây
    public Sprite[] numberSprites;   // Ảnh 3, 2, 1
    
    [Header("---- WIN CANVAS UI ----")]
    public TMP_Text winTimeText;     // Kéo thẻ RaceTimeText ở WinCanvas vào đây

    public static GameManager Instance;

    [Header("---- CÀI ĐẶT ÂM THANH ----")]
    public AudioClip countdownGoSound;  // Âm thanh 3-2-1 bíp + GO (khoảng 4 giây)
    private AudioSource audioSource;

    [Header("---- CÀI ĐẶT RESPONSIVE UI ----")]
    private CanvasResponsive canvasResponsive; // Reference tới script CanvasResponsive

    [Header("---- CÀI ĐẶT CAMERA ----")]
    private TopDownCameraFollow cameraFollow;   // Script camera

    // [BIẾN HỆ THỐNG]
    private float raceTime = 0f;
    private bool isRacing = false;
    private float uiUpdateTimer = 0f;

    // [BIẾN QUẢN LÍ XE]
    private ControlSpeedAnim playerCar;           // Script lái xe player
    private Animator playerAnimator;              // Animator xe player
    private AICarController[] aiCars;             // Mảng các xe AI
    private Rigidbody2D[] aiCarRigidbodies;       // Rigidbody của các xe AI

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        raceTime = 0f;
        isRacing = false;

        // Tự động tìm timeText nếu bị rớt reference
        if (timeText == null)
        {
            TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in allTexts)
            {
                if (t.name == "Txt_Time")
                {
                    timeText = t;
                    break;
                }
            }
        }

        if (timeText != null)
        {
            timeText.text = "00:00:00";
        }

        PlayMapBGM();

        // ===== SETUP AUDIO =====
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // ===== SETUP CANVAS & CAMERA =====
        canvasResponsive = FindFirstObjectByType<CanvasResponsive>();
        cameraFollow = FindFirstObjectByType<TopDownCameraFollow>();

        // ===== AUTO FIND WIN TIME TEXT NẾU CHƯA GÁN =====
        if (winTimeText == null)
        {
            GameObject findFinishCanvas = GameObject.Find("Victory Canvas"); // Thay tên nếu nó đổi
            if(findFinishCanvas == null) findFinishCanvas = GameObject.Find("Win Map Canvas");
            if(findFinishCanvas == null) findFinishCanvas = GameObject.Find("FinishCanvas");

            if (findFinishCanvas != null)
            {
                TMP_Text[] texts = findFinishCanvas.GetComponentsInChildren<TMP_Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i].name == "RaceTimeText")
                    {
                        winTimeText = texts[i];
                        break;
                    }
                }
            }
        }

        // ===== SETUP PLAYER CAR =====
        playerCar = FindFirstObjectByType<ControlSpeedAnim>();
        if (playerCar != null)
        {
            playerAnimator = playerCar.GetComponent<Animator>();
        }

        // ===== SETUP AI CARS =====
        aiCars = FindObjectsOfType<AICarController>();
        aiCarRigidbodies = new Rigidbody2D[aiCars.Length];
        for (int i = 0; i < aiCars.Length; i++)
        {
            aiCarRigidbodies[i] = aiCars[i].GetComponent<Rigidbody2D>();
        }

        // Bắt đầu countdown (Time.timeScale sẽ được set = 0 trong coroutine)
        StartCoroutine(StartCountdown());
    }

    void Update()
    {
        if (isRacing)
        {
            raceTime += Time.deltaTime;

            // GIẢM TỐC ĐỘ CẬP NHẬT UI
            uiUpdateTimer += Time.deltaTime;
            if (uiUpdateTimer >= 0.05f)
            {
                uiUpdateTimer = 0f;

                int minutes = Mathf.FloorToInt(raceTime / 60F);
                int seconds = Mathf.FloorToInt(raceTime % 60F);
                int milliseconds = Mathf.FloorToInt((raceTime * 100F) % 100F);

                if (timeText != null)
                {
                    timeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
                }
            }
        }
    }

    IEnumerator StartCountdown()
    {
        // ===== 1. DỪNG GAME BẰNG TIME.TIMESCALE =====
        Time.timeScale = 0f;  // Dừng mọi vật lý & animation
        
        // Hiển thị UI countdown
        if (numberDisplay) numberDisplay.gameObject.SetActive(true);
        if (goHolder) goHolder.SetActive(false);
        if (canvasResponsive != null) canvasResponsive.ResetNumberAndGO();

        yield return new WaitForSecondsRealtime(0.3f);

        // ===== 2. PHÁT ÂM THANH 3-2-1-GO =====
        if (countdownGoSound != null)
        {
            audioSource.PlayOneShot(countdownGoSound);
        }

        float totalDuration = countdownGoSound != null ? countdownGoSound.length : 4f;
        float countdownPortion = 3f;  // 3 giây cho 3-2-1
        float startTime = Time.realtimeSinceStartup;
        int currentNumber = -1;
        bool goShown = false;

        while (Time.realtimeSinceStartup - startTime < totalDuration)
        {
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            
            // ===== HIỂN THỊ 3-2-1 (0-3 giây) =====
            if (elapsedTime < countdownPortion)
            {
                int newNumber = Mathf.FloorToInt(elapsedTime);
                if (newNumber != currentNumber && newNumber < numberSprites.Length)
                {
                    if (numberDisplay)
                    {
                        numberDisplay.sprite = numberSprites[newNumber];
                        numberDisplay.SetNativeSize();
                    }
                    currentNumber = newNumber;
                }
            }
            // ===== HIỂN THỊ GO (từ giây thứ 3 trở đi) =====
            else if (!goShown)
            {
                if (numberDisplay) numberDisplay.gameObject.SetActive(false);
                if (goHolder) goHolder.SetActive(true);
                goShown = true;
            }

            yield return null;
        }

        // ===== 3. BẮT ĐẦU GAME LẠI =====
        Time.timeScale = 1f;  // Resume game
        
        if (goHolder) goHolder.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        isRacing = true;
    }

    /// <summary>
    /// Dừng tất cả các xe (Player + AI) - không cần gọi, timeScale đã dừng mọi thứ
    /// </summary>
    private void StopAllCars()
    {
        // Code này giữ lại nhưng không dùng, vì Time.timeScale = 0 dừng mọi thứ rồi
    }

    /// <summary>
    /// Dừng đếm thời gian và cập nhật UI bảng Win
    /// </summary>
    public void StopRaceTime()
    {
        isRacing = false;
        
        // Đẩy thẳng thời gian cuối cùng sang màn hình Win Nếu có gắn thẻ winTimeText
        if (winTimeText != null && timeText != null)
        {
            winTimeText.text = timeText.text;
        }
    }

    /// <summary>
    /// Khởi động tất cả các xe (Player + AI) - không cần gọi, timeScale đã khởi động mọi thứ
    /// </summary>
    private void StartAllCars()
    {
        // Code này giữ lại nhưng không dùng, vì Time.timeScale = 1 khởi động mọi thứ rồi
    }

    private void PlayMapBGM()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
        string path = "Audio/";
        
        if (sceneName.Contains("map 1") || sceneName.Contains("map1") || sceneName.Contains("rung")) 
            path += "map1_bgm";
        else if (sceneName.Contains("map 2") || sceneName.Contains("map2") || sceneName.Contains("sa mac") || sceneName.Contains("samac")) 
            path += "map2_bgm";
        else if (sceneName.Contains("map 3") || sceneName.Contains("map3") || sceneName.Contains("thien dinh") || sceneName.Contains("thiên đình")) 
            path += "map3_bgm";
        else 
            return; // Không nhận diện được Map, không bật nhạc

        AudioClip bgmClip = Resources.Load<AudioClip>(path);
        if (bgmClip != null)
        {
            AudioSource bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0.45f; // BGM nhỏ lại tí xíu
            bgmSource.playOnAwake = true;
            bgmSource.spatialBlend = 0f; // 2D Sound
            bgmSource.Play();
        }
    }
}
