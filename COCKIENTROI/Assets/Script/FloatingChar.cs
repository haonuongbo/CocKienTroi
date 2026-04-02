using UnityEngine;
using UnityEngine.UI;

public class FloatingChar : MonoBehaviour
{
    [Header("Animation Nhún")]
    public Image charPreviewImage; // Gán charPreview Image vào đây
    public float amplitude = 20.7f; // Độ cao nhún (pixel)
    public float frequency = 1.31f;   // Tốc độ nhún (chu kỳ mỗi giây)
    public bool isFloating = true; // Bật/tắt animation

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        // Nếu không gán charPreviewImage, tự tìm RectTransform của object này
        if (charPreviewImage != null)
        {
            rectTransform = charPreviewImage.GetComponent<RectTransform>();
        }
        else
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        ResetStartPosition();
    }

    // --- MỚI: Method để reset startPos khi vị trí thay đổi ---
    public void ResetStartPosition()
    {
        if (rectTransform != null)
        {
            startPos = rectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        if (!isFloating || rectTransform == null) return;

        // Công thức: Vị trí mới = Vị trí gốc (Y từ CharacterSelection) + (Hướng lên * Dao động Sin)
        Vector2 tempPos = startPos;
        tempPos.y += Mathf.Sin(Time.time * Mathf.PI * frequency) * amplitude;
        rectTransform.anchoredPosition = tempPos;
    }
}