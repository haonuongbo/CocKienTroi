using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý UI Responsive - Thích ứng kích thước và vị trí UI theo độ phân giải và aspect ratio
/// </summary>
public class CanvasResponsive : MonoBehaviour
{
    [Header("---- CÀI ĐẶT RESPONSIVE ----")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasScaler canvasScaler;
    
    [Header("---- CÀI ĐẶT SCALE UI ----")]
    [SerializeField] private float baseWidth = 1920f;        // Độ phân giải cơ sở (chiều rộng)
    [SerializeField] private float baseHeight = 1080f;       // Độ phân giải cơ sở (chiều cao)
    [SerializeField] private float minScale = 0.5f;          // Scale tối thiểu
    [SerializeField] private float maxScale = 2f;            // Scale tối đa

    [Header("---- CÀI ĐẶT VỊ TRÍ UI ----")]
    [SerializeField] private RectTransform numberDisplay;    // Image số 3-2-1
    [SerializeField] private RectTransform goHolder;         // Chữ GO
    [SerializeField] private RectTransform timeText;         // Timer

    [Header("---- VỊ TRÍ MẶC ĐỊNH ----")]
    [SerializeField] private Vector2 numberDisplayOffset = Vector2.zero;
    [SerializeField] private Vector2 goHolderOffset = Vector2.zero;
    [SerializeField] private Vector2 timeTextOffset = Vector2.zero;

    [Header("---- CÀI ĐẶT TIME TEXT ----")]
    [SerializeField] private bool lockTimeTextPosition = true;  // Giữ nguyên vị trí timeText?
    [SerializeField] private bool lockTimeTextScale = true;     // Giữ nguyên kích thước timeText?

    // Thông tin màn hình
    private float currentScale = 1f;
    private Vector2 lastScreenSize;
    private float currentAspectRatio;

    void Start()
    {
        // Lấy Canvas và CanvasScaler nếu không được assign
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvasScaler == null) canvasScaler = GetComponent<CanvasScaler>();

        // Cập nhật lần đầu tiên
        UpdateResponsiveUI();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    void Update()
    {
        // Kiểm tra nếu độ phân giải thay đổi
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
        if (currentScreenSize != lastScreenSize)
        {
            UpdateResponsiveUI();
            lastScreenSize = currentScreenSize;
        }
    }

    /// <summary>
    /// Cập nhật toàn bộ UI responsive
    /// </summary>
    private void UpdateResponsiveUI()
    {
        if (canvas == null) return;

        // Tính toán scale dựa trên độ phân giải hiện tại
        CalculateScale();

        // Cập nhật Canvas Scaler
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(baseWidth, baseHeight);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            
            // Điều chỉnh MatchWidthOrHeight dựa trên aspect ratio
            canvasScaler.matchWidthOrHeight = currentAspectRatio > (baseWidth / baseHeight) ? 0 : 1;
        }

        // Cập nhật vị trí các UI elements
        UpdateUIElementsPosition();

        Debug.Log($"[CanvasResponsive] Cập nhật UI | Scale: {currentScale:F2} | Aspect Ratio: {currentAspectRatio:F2} | Screen: {Screen.width}x{Screen.height}");
    }

    /// <summary>
    /// Tính toán scale UI dựa trên độ phân giải
    /// </summary>
    private void CalculateScale()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Tính aspect ratio hiện tại
        currentAspectRatio = screenWidth / screenHeight;

        // Tính scale dựa trên tỷ lệ với độ phân giải cơ sở
        float widthRatio = screenWidth / baseWidth;
        float heightRatio = screenHeight / baseHeight;

        // Lấy tỷ lệ nhỏ hơn để đảm bảo UI vừa với màn hình
        currentScale = Mathf.Min(widthRatio, heightRatio);

        // Giới hạn scale trong khoảng min-max
        currentScale = Mathf.Clamp(currentScale, minScale, maxScale);
    }

    /// <summary>
    /// Cập nhật vị trí các UI elements
    /// </summary>
    private void UpdateUIElementsPosition()
    {
        // Cập nhật Number Display (Số 3-2-1)
        if (numberDisplay != null)
        {
            UpdateElementSize(numberDisplay, 1f); // Tỷ lệ 100%
            UpdateElementPosition(numberDisplay, numberDisplayOffset);
        }

        // Cập nhật GO Holder
        if (goHolder != null)
        {
            UpdateElementSize(goHolder, 1f);
            UpdateElementPosition(goHolder, goHolderOffset);
        }

        // Cập nhật Time Text
        if (timeText != null)
        {
            // Chỉ cập nhật kích thước nếu không lock
            if (!lockTimeTextScale)
            {
                UpdateElementSize(timeText, 0.8f);
            }
            
            // Chỉ cập nhật vị trí nếu không lock
            if (!lockTimeTextPosition)
            {
                UpdateElementPosition(timeText, timeTextOffset);
            }
        }
    }

    /// <summary>
    /// Cập nhật kích thước element
    /// </summary>
    private void UpdateElementSize(RectTransform element, float sizeMultiplier = 1f)
    {
        if (element == null) return;

        // Scale được áp dụng thông qua Canvas Scaler
        // Nếu muốn custom size theo aspect ratio, có thể thêm logic ở đây
        element.localScale = Vector3.one * (currentScale * sizeMultiplier);
    }

    /// <summary>
    /// Cập nhật vị trí element dựa trên offset
    /// </summary>
    private void UpdateElementPosition(RectTransform element, Vector2 offset)
    {
        if (element == null) return;

        // Áp dụng offset vị trí
        element.anchoredPosition = offset;
    }

    /// <summary>
    /// Lấy current scale của UI
    /// </summary>
    public float GetCurrentScale()
    {
        return currentScale;
    }

    /// <summary>
    /// Lấy aspect ratio hiện tại
    /// </summary>
    public float GetCurrentAspectRatio()
    {
        return currentAspectRatio;
    }

    /// <summary>
    /// Hàm hỗ trợ: Điều chỉnh thủ công vị trí UI elements (có thể gọi từ ngoài nếu cần)
    /// </summary>
    public void SetElementOffset(RectTransform element, Vector2 newOffset)
    {
        if (element == null) return;
        element.anchoredPosition = newOffset;
    }

    /// <summary>
    /// Reset UI về vị trí mặc định
    /// </summary>
    public void ResetToDefaults()
    {
        numberDisplay.anchoredPosition = numberDisplayOffset;
        goHolder.anchoredPosition = goHolderOffset;
        timeText.anchoredPosition = timeTextOffset;
    }

    /// <summary>
    /// Reset chỉ Number Display và GO Holder (không reset TimeText)
    /// </summary>
    public void ResetNumberAndGO()
    {
        if (numberDisplay != null)
            numberDisplay.anchoredPosition = numberDisplayOffset;
        
        if (goHolder != null)
            goHolder.anchoredPosition = goHolderOffset;
        
        // TimeText giữ nguyên vị trí hiện tại
    }
}
