using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("---- CÀI ĐẶT UI ----")]
    public Image numberDisplay;      // Ảnh số 3-2-1
    public GameObject goHolder;      // Cụm chữ GO
    public TMP_Text timeText;        // Kéo cái Txt_Time vào đây

    public Sprite[] numberSprites;   // Ảnh 3, 2, 1

    [Header("---- CÀI ĐẶT XE ----")]
    public Controller playerCar;     // Script lái xe

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

    void Start()
    {
        // Reset thời gian về 0
        raceTime = 0f;
        isRacing = false;

        // Khởi tạo AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Lấy CanvasResponsive script
        canvasResponsive = FindObjectOfType<CanvasResponsive>();
        if (canvasResponsive == null)
        {
            Debug.LogWarning("[GameManager] Không tìm thấy CanvasResponsive script! Vui lòng thêm vào Canvas.");
        }

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
        // 1. KHÓA XE & RESET
        if (playerCar != null) playerCar.enabled = false;
        if (numberDisplay) numberDisplay.gameObject.SetActive(true);
        if (goHolder) goHolder.SetActive(false);

        // Cập nhật UI responsive (chỉ reset Number và GO, không reset TimeText)
        if (canvasResponsive != null)
        {
            canvasResponsive.ResetNumberAndGO();
        }

        // 2. ĐẾM 3 -> 2 -> 1
        for (int i = 0; i < numberSprites.Length; i++)
        {
            if (numberDisplay)
            {
                numberDisplay.sprite = numberSprites[i];
                numberDisplay.SetNativeSize();
            }
            
            // Phát âm thanh bíp
            if (audioSource != null && beepSound != null)
            {
                audioSource.PlayOneShot(beepSound);
            }
            
            yield return new WaitForSeconds(1f);
        }

        // 3. HIỆN CHỮ GO!
        if (numberDisplay) numberDisplay.gameObject.SetActive(false);
        if (goHolder) goHolder.SetActive(true);

        // Phát âm thanh GO
        if (audioSource != null && goSound != null)
        {
            audioSource.PlayOneShot(goSound);
        }

        // 4. CHỜ 1 GIÂY
        yield return new WaitForSeconds(1f);

        // 5. BẮT ĐẦU ĐUA (Start Game)
        if (goHolder) goHolder.SetActive(false);
        if (playerCar != null) playerCar.enabled = true;

        isRacing = true; // Kích hoạt đồng hồ đếm giờ
    }
}
