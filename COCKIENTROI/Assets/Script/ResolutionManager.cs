using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý độ phân giải và cài đặt màn hình toàn cục
/// </summary>
public class ResolutionManager : MonoBehaviour
{
    [Header("---- CÀI ĐẶT ĐỘ PHÂN GIẢI ----")]
    [SerializeField] private Vector2Int[] supportedResolutions = new Vector2Int[]
    {
        // Desktop / common landscape presets
        new Vector2Int(1024, 768),
        new Vector2Int(1280, 720),
        new Vector2Int(1280, 1024),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),

        // Mobile / tablet presets
        new Vector2Int(720, 1280),
        new Vector2Int(750, 1334),
        new Vector2Int(1080, 1920),
        new Vector2Int(1125, 2436),
        new Vector2Int(1170, 2532),
        new Vector2Int(1179, 2556),
        new Vector2Int(1242, 2688),
        new Vector2Int(1284, 2778),
        new Vector2Int(1440, 3040),
        new Vector2Int(1440, 3200),
        new Vector2Int(1536, 2048),
        new Vector2Int(1668, 2224),
        new Vector2Int(2048, 2732)
    };

    [SerializeField] private int defaultResolutionIndex = 4; // 1920x1080
    [SerializeField] private bool fullscreen = true;
    [SerializeField] private bool forceLandscape = true;
    [SerializeField] private int targetFramerate = 60;

    [Header("---- CÀI ĐẶT QUALITY ----")]
    [SerializeField] private int qualityLevel = 2; // 0-5

    // Singleton
    private static ResolutionManager instance;

    void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyOrientation();

        // Áp dụng cài đặt ban đầu
        ApplyResolution(defaultResolutionIndex);
        ApplyQualityLevel(qualityLevel);
    }

    /// <summary>
    /// Áp dụng độ phân giải
    /// </summary>
    public void ApplyResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= supportedResolutions.Length)
        {
            Debug.LogError($"[ResolutionManager] Index {resolutionIndex} ngoài phạm vi!");
            return;
        }

        Vector2Int resolution = supportedResolutions[resolutionIndex];
        resolution = NormalizeResolutionForOrientation(resolution);
        Screen.SetResolution(resolution.x, resolution.y, fullscreen);

        Debug.Log($"[ResolutionManager] Áp dụng độ phân giải: {resolution.x}x{resolution.y} | Fullscreen: {fullscreen}");
    }

    /// <summary>
    /// Thay đổi độ phân giải thủ công
    /// </summary>
    public void SetResolution(int width, int height, bool isFullscreen)
    {
        Vector2Int resolution = NormalizeResolutionForOrientation(new Vector2Int(width, height));
        Screen.SetResolution(width, height, isFullscreen);
        fullscreen = isFullscreen;

        Screen.SetResolution(resolution.x, resolution.y, isFullscreen);

        Debug.Log($"[ResolutionManager] Thay đổi độ phân giải thành: {resolution.x}x{resolution.y} | Fullscreen: {isFullscreen}");
    }

    /// <summary>
    /// Áp dụng mức chất lượng
    /// </summary>
    public void ApplyQualityLevel(int level)
    {
        if (level < 0 || level >= QualitySettings.names.Length)
        {
            Debug.LogError($"[ResolutionManager] Mức chất lượng {level} không tồn tại!");
            return;
        }

        QualitySettings.SetQualityLevel(level);
        qualityLevel = level;

        Debug.Log($"[ResolutionManager] Áp dụng chất lượng: {QualitySettings.names[level]}");
    }

    /// <summary>
    /// Đặt target framerate
    /// </summary>
    public void SetTargetFramerate(int fps)
    {
        Application.targetFrameRate = fps;
        targetFramerate = fps;

        Debug.Log($"[ResolutionManager] Đặt target framerate: {fps}");
    }

    /// <summary>
    /// Toggle fullscreen
    /// </summary>
    public void ToggleFullscreen()
    {
        fullscreen = !fullscreen;
        Vector2Int resolution = supportedResolutions[defaultResolutionIndex];
        resolution = NormalizeResolutionForOrientation(resolution);
        Screen.SetResolution(resolution.x, resolution.y, fullscreen);

        Debug.Log($"[ResolutionManager] Fullscreen: {fullscreen}");
    }

    /// <summary>
    /// Lấy danh sách độ phân giải hỗ trợ
    /// </summary>
    public Vector2Int[] GetSupportedResolutions()
    {
        return supportedResolutions;
    }

    /// <summary>
    /// Lấy độ phân giải hiện tại
    /// </summary>
    public Vector2Int GetCurrentResolution()
    {
        return new Vector2Int(Screen.width, Screen.height);
    }

    /// <summary>
    /// Lấy trạng thái fullscreen
    /// </summary>
    public bool IsFullscreen()
    {
        return fullscreen;
    }

    /// <summary>
    /// Lấy instance
    /// </summary>
    public static ResolutionManager Instance
    {
        get { return instance; }
    }

    private void ApplyOrientation()
    {
        if (!forceLandscape)
            return;

        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }

    private Vector2Int NormalizeResolutionForOrientation(Vector2Int resolution)
    {
        if (!forceLandscape)
            return resolution;

        if (resolution.y > resolution.x)
            return new Vector2Int(resolution.y, resolution.x);

        return resolution;
    }
}
