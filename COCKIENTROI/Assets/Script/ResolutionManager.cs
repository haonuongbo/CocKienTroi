using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý độ phân giải và cài đặt màn hình toàn cục.
/// Tự động dùng độ phân giải native của thiết bị và điều chỉnh
/// CanvasScaler theo tỉ lệ màn hình thực tế.
/// </summary>
public class ResolutionManager : MonoBehaviour
{
    [Header("---- HƯỚNG MÀN HÌNH ----")]
    [SerializeField] private bool forceLandscape = true;

    [Header("---- CANVAS SCALER (UI ADAPT) ----")]
    // Độ phân giải thiết kế UI gốc (landscape)
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    // 0 = match width, 1 = match height, 0.5 = blend
    [SerializeField] [Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;

    [Header("---- HIỆU NĂNG ----")]
    [SerializeField] private int targetFramerate = 60;
    // Giới hạn DPI render để tiết kiệm GPU (0 = không giới hạn)
    [SerializeField] [Range(0, 600)] private int maxRenderDpi = 400;

    [Header("---- QUALITY ----")]
    [SerializeField] private int qualityLevel = 2;

    // Singleton
    public static ResolutionManager Instance { get; private set; }

    // Tỉ lệ màn hình thực tế tính sau khi Awake
    public float ScreenAspect { get; private set; }
    public Vector2Int NativeResolution { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Đặt orientation trước, sau đó chờ orientation ổn định mới apply resolution
        ApplyOrientation();
        ApplyQualityLevel(qualityLevel);
        Application.targetFrameRate = targetFramerate;

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    void Start()
    {
        // Dùng coroutine để chờ orientation thực sự thay đổi trước khi đọc Screen size
        StartCoroutine(ApplyAfterOrientationSettled());
    }

    /// <summary>
    /// Chờ cho đến khi màn hình đã xoay sang landscape rồi mới apply resolution và canvas.
    /// </summary>
    private IEnumerator ApplyAfterOrientationSettled()
    {
        if (forceLandscape)
        {
            // Chờ tối đa 1 giây cho orientation thay đổi
            float timeout = 1f;
            float elapsed = 0f;
            while (Screen.width < Screen.height && elapsed < timeout)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }

            // Thêm 1 frame nữa để Screen.width/height ổn định hoàn toàn
            yield return new WaitForEndOfFrame();
        }

        UseNativeResolution();
        AdaptAllCanvasScalers();
    }

    // ─────────────────────────────────────────────
    // ĐỘ PHÂN GIẢI NATIVE
    // ─────────────────────────────────────────────

    /// <summary>
    /// Dùng độ phân giải native của thiết bị. Nếu maxRenderDpi > 0,
    /// scale xuống để giữ hiệu năng trên màn hình QHD/4K.
    /// </summary>
    private void UseNativeResolution()
    {
        int nativeW = Screen.currentResolution.width;
        int nativeH = Screen.currentResolution.height;

        // Trên mobile Screen.currentResolution trả về native hardware res.
        // Trên editor dùng Screen.width/height thay thế.
#if UNITY_EDITOR
        nativeW = Screen.width;
        nativeH = Screen.height;
#endif

        if (forceLandscape && nativeH > nativeW)
            (nativeW, nativeH) = (nativeH, nativeW);

        // Scale xuống nếu DPI quá cao
        int renderW = nativeW;
        int renderH = nativeH;
        if (maxRenderDpi > 0 && Screen.dpi > maxRenderDpi)
        {
            float scale = maxRenderDpi / Screen.dpi;
            renderW = Mathf.RoundToInt(nativeW * scale);
            renderH = Mathf.RoundToInt(nativeH * scale);
        }

        NativeResolution = new Vector2Int(nativeW, nativeH);
        ScreenAspect = (float)renderW / renderH;

    #if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
        // Trên mobile VÀ trong Editor Simulator, tuyệt đối không dùng SetResolution 
        // để tránh phá vỡ khung hình (gây viền xanh/đen) của Simulator.
        renderW = Screen.width;
        renderH = Screen.height;
        NativeResolution = new Vector2Int(renderW, renderH);
        ScreenAspect = (float)renderW / renderH;
    #else
        Screen.SetResolution(renderW, renderH, FullScreenMode.FullScreenWindow);
    #endif
        Debug.Log($"[ResolutionManager] Native: {nativeW}x{nativeH} | Render: {renderW}x{renderH} | DPI: {Screen.dpi:F0} | Aspect: {ScreenAspect:F3}");
    }

    // ─────────────────────────────────────────────
    // CANVAS SCALER AUTO-ADAPT
    // ─────────────────────────────────────────────

    /// <summary>
    /// Tìm tất cả CanvasScaler trong scene và cấu hình Scale With Screen Size
    /// với match value phù hợp tỉ lệ màn hình thực tế.
    /// </summary>
    public void AdaptAllCanvasScalers()
    {
        CanvasScaler[] scalers = FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);
        foreach (CanvasScaler scaler in scalers)
            ConfigureCanvasScaler(scaler);

        Debug.Log($"[ResolutionManager] Đã adapt {scalers.Length} CanvasScaler(s).");
    }

    /// <summary>
    /// Cấu hình một CanvasScaler cụ thể.
    /// </summary>
    public void ConfigureCanvasScaler(CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        // Tự động tính match dựa trên tỉ lệ màn hình so với reference
        float refAspect = referenceResolution.x / referenceResolution.y;
        float deviceAspect = ScreenAspect > 0 ? ScreenAspect : (float)Screen.width / Screen.height;

        // Nếu device rộng hơn reference (điện thoại siêu dài) → ưu tiên match height (1) để chống cắt dọc
        // Nếu device hẹp hơn reference (iPad) → dùng 0.5 (Blend) để phóng to UI, giúp chữ dễ đọc hơn và giao diện dàn đều ra mép
        float autoMatch = deviceAspect >= refAspect ? 1f : 0.5f;

        // Bỏ qua Lerp, ép buộc dùng autoMatch tuyệt đối để đảm bảo UI không bị sai lệch trên iPhone
        scaler.matchWidthOrHeight = autoMatch;
    }

    // ─────────────────────────────────────────────
    // ORIENTATION
    // ─────────────────────────────────────────────

    private void ApplyOrientation()
    {
        if (forceLandscape)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
        else
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
    }

    // ─────────────────────────────────────────────
    // QUALITY / FRAMERATE
    // ─────────────────────────────────────────────

    public void ApplyQualityLevel(int level)
    {
        if (level < 0 || level >= QualitySettings.names.Length)
        {
            Debug.LogError($"[ResolutionManager] Mức chất lượng {level} không tồn tại!");
            return;
        }
        QualitySettings.SetQualityLevel(level);
        qualityLevel = level;
        Debug.Log($"[ResolutionManager] Chất lượng: {QualitySettings.names[level]}");
    }

    public void SetTargetFramerate(int fps)
    {
        Application.targetFrameRate = fps;
        targetFramerate = fps;
        Debug.Log($"[ResolutionManager] Target framerate: {fps}");
    }

    // ─────────────────────────────────────────────
    // PUBLIC HELPERS
    // ─────────────────────────────────────────────

    public Vector2Int GetCurrentResolution() => new Vector2Int(Screen.width, Screen.height);
    public bool IsFullscreen() => Screen.fullScreen;

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isActiveAndEnabled)
            return;

        // Scene Loading có thanh progress bar căn chỉnh chính xác theo video nền.
        if (scene.name == "Loading")
        {
            Debug.Log($"[ResolutionManager] Bỏ qua adapt CanvasScaler cho scene '{scene.name}'.");
            return;
        }

        // Tất cả scene khác (kể cả Map 1, 2, 3) → adapt bình thường giống Character Selection
        AdaptAllCanvasScalers();
    }
}
