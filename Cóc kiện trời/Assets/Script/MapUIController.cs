using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapUIController : MonoBehaviour
{
    [Header("Race Scene UI References")]
    [SerializeField] private TextMeshProUGUI currentMapText;     // Text hiển thị "Map 1", "Map 2", etc
    [SerializeField] private Image nextMapUnlockIcon;            // Icon để hiển thị map mới unlock
    [SerializeField] private TextMeshProUGUI nextMapText;        // Text "Map X Unlocked!"
    [SerializeField] private GameObject unlockNotification;      // Panel thông báo unlock

    [Header("Animation Settings")]
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Colors & Visuals")]
    [SerializeField] private Color unlockColor = new Color(1f, 0.84f, 0f); // Vàng/gold
    [SerializeField] private AudioClip unlockSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Cập nhật text map hiện tại nếu có
        if (currentMapText != null && TryGetComponent<RaceWinHandler>(out var raceHandler))
        {
            // Sẽ được set từ RaceWinHandler
        }

        // Ẩn notification ban đầu
        if (unlockNotification != null)
        {
            unlockNotification.SetActive(false);
        }
    }

    /// <summary>
    /// Gọi hàm này từ RaceWinHandler khi thắng map để hiển thị unlock animation
    /// </summary>
    public void OnMapWon(int currentMapNumber)
    {
        int nextMapNumber = currentMapNumber + 1;

        // Kiểm tra xem map tiếp theo có được unlock không
        if (MapManager.Instance.IsMapUnlocked(nextMapNumber) && nextMapNumber <= 3)
        {
            ShowUnlockNotification(nextMapNumber);
        }
    }

    /// <summary>
    /// Hiển thị thông báo unlock map mới
    /// </summary>
    private void ShowUnlockNotification(int mapNumber)
    {
        if (unlockNotification != null)
        {
            StartCoroutine(UnlockAnimationSequence(mapNumber));
        }
    }

    private System.Collections.IEnumerator UnlockAnimationSequence(int mapNumber)
    {
        // 1. Hiện notification
        unlockNotification.SetActive(true);

        // Cập nhật text
        if (nextMapText != null)
        {
            nextMapText.text = $"🔓 Map {mapNumber} Unlocked!";
        }

        // 2. Scale up animation
        var rectTransform = unlockNotification.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.zero;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                float progress = t / fadeDuration;
                float scale = scaleCurve.Evaluate(progress);
                rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }
            rectTransform.localScale = Vector3.one;
        }

        // 3. Phát âm thanh
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        // 4. Hiệu ứng shine/glow (tuỳ chọn)
        if (nextMapUnlockIcon != null)
        {
            StartCoroutine(ShineEffect(nextMapUnlockIcon));
        }

        // 5. Hiển thị notification trong thời gian định sẵn
        yield return new WaitForSeconds(notificationDuration);

        // 6. Fade out
        var canvasGroup = unlockNotification.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = unlockNotification.AddComponent<CanvasGroup>();
        }

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float progress = t / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            yield return null;
        }

        unlockNotification.SetActive(false);
        canvasGroup.alpha = 1f; // Reset cho lần next
    }

    /// <summary>
    /// Hiệu ứng shine cho icon (làm sáng bóng bẩy)
    /// </summary>
    private System.Collections.IEnumerator ShineEffect(Image icon)
    {
        Color originalColor = icon.color;
        float duration = 0.6f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = t / duration;
            float brightness = Mathf.Lerp(1f, 1.5f, Mathf.Sin(progress * Mathf.PI));
            icon.color = originalColor * brightness;
            yield return null;
        }

        icon.color = originalColor;
    }

    /// <summary>
    /// Update hiển thị map hiện tại
    /// </summary>
    public void SetCurrentMapDisplay(int mapNumber)
    {
        if (currentMapText != null)
        {
            currentMapText.text = $"Map {mapNumber}";
        }
    }

    /// <summary>
    /// Hiển thị popup thông báo tùy chỉnh
    /// </summary>
    public void ShowNotification(string message, float duration = 2f)
    {
        if (nextMapText != null)
        {
            nextMapText.text = message;
        }

        if (unlockNotification != null)
        {
            StartCoroutine(ShowNotificationCoroutine(duration));
        }
    }

    private System.Collections.IEnumerator ShowNotificationCoroutine(float duration)
    {
        unlockNotification.SetActive(true);
        var canvasGroup = unlockNotification.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = unlockNotification.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(duration);

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        unlockNotification.SetActive(false);
    }
}
