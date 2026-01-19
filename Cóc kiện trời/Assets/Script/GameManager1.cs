using System.Collections;
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

    [Header("---- CÀI ĐẶT ÂM THANH ----")]
    public AudioClip beepSound;      // Âm thanh bíp cho 3-2-1
    public AudioClip goSound;        // Âm thanh GO riêng
    private AudioSource audioSource;

    [Header("---- CÀI ĐẶT RESPONSIVE UI ----")]
    private CanvasResponsive canvasResponsive; // Reference tới script CanvasResponsive

    // [BIẾN HỆ THỐNG]
    private float raceTime = 0f;     // Biến lưu thời gian chạy
    private bool isRacing = false;   // Biến cờ: True = Đang đua, False = Dừng
    private float uiUpdateTimer = 0f;
    private ControlSpeedAnim  playerCar;                // Script lái xe
    private TopDownCameraFollow cameraFollow;   // Script camera

    [Header("---- Disalbe Animation ----")]
    public Animator playerAnimator;

    void Start()
    {
        raceTime = 0f;
        isRacing = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        canvasResponsive = FindObjectOfType<CanvasResponsive>();

        playerCar = FindObjectOfType<ControlSpeedAnim>();
        if (playerCar != null)
            playerAnimator = playerCar.GetComponent<Animator>();

        cameraFollow = FindObjectOfType<TopDownCameraFollow>();

        // Disable UI input during countdown
        if (EventSystem.current != null)
            EventSystem.current.enabled = false;

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
        // 1. DISABLE GAMEPLAY
        if (playerCar != null) playerCar.enabled = false;
        if (playerAnimator != null) playerAnimator.speed = 0f;
        if (cameraFollow != null) cameraFollow.enabled = true;

        if (numberDisplay) numberDisplay.gameObject.SetActive(true);
        if (goHolder) goHolder.SetActive(false);

        if (canvasResponsive != null)
            canvasResponsive.ResetNumberAndGO();

        // 2. COUNTDOWN
        for (int i = 0; i < numberSprites.Length; i++)
        {
            if (numberDisplay)
            {
                numberDisplay.sprite = numberSprites[i];
                numberDisplay.SetNativeSize();
            }

            if (beepSound != null)
                audioSource.PlayOneShot(beepSound);

            yield return new WaitForSeconds(1f);
        }

        // 3. GO
        if (numberDisplay) numberDisplay.gameObject.SetActive(false);
        if (goHolder) goHolder.SetActive(true);

        if (goSound != null)
            audioSource.PlayOneShot(goSound);

        yield return new WaitForSeconds(1f);

        // 4. ENABLE GAMEPLAY
        if (goHolder) goHolder.SetActive(false);
        if (playerCar != null) playerCar.enabled = true;
        if (playerAnimator != null) playerAnimator.speed = 1f;
        if (cameraFollow != null) cameraFollow.enabled = true;

        if (EventSystem.current != null)
            EventSystem.current.enabled = true;

        isRacing = true;
    }

}
