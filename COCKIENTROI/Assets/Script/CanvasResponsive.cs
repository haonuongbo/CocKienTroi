using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý UI Responsive cho HUD trong các Map scene (đua xe).
/// 
/// QUAN TRỌNG: HUD_Canvas là CHILD Canvas của JoyStick HUD (root).
/// → CanvasScaler trên HUD_Canvas bị Unity bỏ qua.
/// → Cần ép RectTransform của HUD_Canvas stretch fill theo parent.
/// → ResolutionManager xử lý CanvasScaler của JoyStick HUD (root).
/// </summary>
public class CanvasResponsive : MonoBehaviour
{
    [Header("---- COUNTDOWN UI ----")]
    [SerializeField] private RectTransform numberDisplay;
    [SerializeField] private RectTransform goHolder;
    [SerializeField] private RectTransform timeText;

    [Header("---- VỊ TRÍ MẶC ĐỊNH COUNTDOWN ----")]
    [SerializeField] private Vector2 numberDisplayOffset = Vector2.zero;
    [SerializeField] private Vector2 goHolderOffset = Vector2.zero;
    [SerializeField] private Vector2 timeTextOffset = Vector2.zero;

    [Header("---- CÀI ĐẶT TIME TEXT ----")]
    [SerializeField] private bool lockTimeTextPosition = true;
    [SerializeField] private bool lockTimeTextScale = true;

    void Start()
    {
        // HUD_Canvas là child canvas → ép stretch fill theo parent (JoyStick HUD)
        // để luôn khớp kích thước màn hình trên mọi thiết bị
        StretchToFillParent();
    }

    /// <summary>
    /// Ép RectTransform của HUD_Canvas stretch fill toàn bộ parent canvas.
    /// 
    /// VẤN ĐỀ: HUD_Canvas có RectTransform cố định 1920x1080 (anchor: left, pos: 960,540).
    /// Trên 16:9 → khớp hoàn hảo.
    /// Trên iPad (4:3) → parent canvas rộng/cao khác → HUD_Canvas không fill → lệch.
    /// 
    /// FIX: Đặt anchor stretch (0,0)-(1,1), offset = 0 → HUD_Canvas luôn = parent size.
    /// </summary>
    private void StretchToFillParent()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;

        // Kiểm tra xem có phải child canvas không
        Canvas myCanvas = GetComponent<Canvas>();
        if (myCanvas != null && !myCanvas.isRootCanvas)
        {
            // Ép stretch fill toàn bộ parent
            rt.anchorMin = Vector2.zero;        // (0, 0)
            rt.anchorMax = Vector2.one;          // (1, 1)
            rt.offsetMin = Vector2.zero;         // Left, Bottom = 0
            rt.offsetMax = Vector2.zero;         // Right, Top = 0
            rt.pivot = new Vector2(0.5f, 0.5f);

            Debug.Log("[CanvasResponsive] HUD_Canvas là child canvas → đã ép stretch fill theo parent.");
        }
    }

    public float GetCurrentScale() => 1f;
    public float GetCurrentAspectRatio() => (float)Screen.width / Screen.height;

    public void SetElementOffset(RectTransform element, Vector2 newOffset)
    {
        if (element != null) element.anchoredPosition = newOffset;
    }

    public void ResetToDefaults()
    {
        if (numberDisplay != null) numberDisplay.anchoredPosition = numberDisplayOffset;
        if (goHolder != null) goHolder.anchoredPosition = goHolderOffset;
        if (timeText != null) timeText.anchoredPosition = timeTextOffset;
    }

    public void ResetNumberAndGO()
    {
        if (numberDisplay != null) numberDisplay.anchoredPosition = numberDisplayOffset;
        if (goHolder != null) goHolder.anchoredPosition = goHolderOffset;
    }
}
