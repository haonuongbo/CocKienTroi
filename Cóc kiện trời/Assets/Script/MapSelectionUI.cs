using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Map Button References")]
    [SerializeField] private Button[] mapButtons;           // Nút cho Map 1, Map 2, Map 3
    [SerializeField] private Image[] mapImageDisplays;      // Ảnh hiển thị của map
    [SerializeField] private GameObject[] lockIconUI;       // Icon khóa trên UI
    [SerializeField] private TextMeshProUGUI[] mapNameText; // Text hiển thị tên map
    [SerializeField] private TextMeshProUGUI[] bestTimeText; // Text hiển thị thời gian tốt nhất

    [Header("Colors")]
    [SerializeField] private Color unlockedTextColor = Color.white;
    [SerializeField] private Color lockedTextColor = Color.gray;

    private void Start()
    {
        // Đảm bảo MapManager được khởi tạo
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager không tìm thấy! Hãy thêm MapManager vào scene.");
            return;
        }

        // Cập nhật UI lần đầu
        RefreshAllMapsUI();

        // Gán event cho các nút
        for (int i = 0; i < mapButtons.Length; i++)
        {
            int mapNum = i + 1;
            mapButtons[i].onClick.AddListener(() => SelectMap(mapNum));
        }
    }

    /// <summary>
    /// Cập nhật toàn bộ UI cho tất cả các map
    /// </summary>
    private void RefreshAllMapsUI()
    {
        for (int i = 0; i < mapButtons.Length; i++)
        {
            UpdateMapUI(i + 1);
        }
    }

    /// <summary>
    /// Cập nhật UI cho một map cụ thể
    /// </summary>
    private void UpdateMapUI(int mapNumber)
    {
        int index = mapNumber - 1;
        bool isUnlocked = MapManager.Instance.IsMapUnlocked(mapNumber);

        // Cập nhật trạng thái nút
        mapButtons[index].interactable = isUnlocked;
        
        // Cập nhật màu text
        if (mapNameText[index] != null)
        {
            mapNameText[index].text = $"Map {mapNumber}";
            mapNameText[index].color = isUnlocked ? unlockedTextColor : lockedTextColor;
        }

        // Cập nhật ảnh
        if (mapImageDisplays[index] != null)
        {
            mapImageDisplays[index].color = isUnlocked ? Color.white : Color.gray;
        }

        // Cập nhật icon khóa
        if (lockIconUI[index] != null)
        {
            lockIconUI[index].SetActive(!isUnlocked);
        }

        // Cập nhật thời gian tốt nhất (nếu có)
        if (bestTimeText[index] != null)
        {
            if (isUnlocked)
            {
                float bestTime = PlayerPrefs.GetFloat($"BestTime_Map_{mapNumber}", -1);
                if (bestTime > 0)
                {
                    int minutes = Mathf.FloorToInt(bestTime / 60f);
                    int seconds = Mathf.FloorToInt(bestTime % 60f);
                    int milliseconds = Mathf.FloorToInt((bestTime * 100f) % 100f);
                    bestTimeText[index].text = $"Best: {minutes:00}:{seconds:00}:{milliseconds:00}";
                }
                else
                {
                    bestTimeText[index].text = "Not completed";
                }
            }
            else
            {
                bestTimeText[index].text = "Locked";
                bestTimeText[index].color = lockedTextColor;
            }
        }
    }

    /// <summary>
    /// Xử lý khi người chơi chọn một map
    /// </summary>
    private void SelectMap(int mapNumber)
    {
        if (MapManager.Instance.IsMapUnlocked(mapNumber))
        {
            Debug.Log($"Đã chọn Map {mapNumber}, đang tải...");
            // TODO: Load scene của map
            // SceneManager.LoadScene($"Map_{mapNumber}");
        }
        else
        {
            Debug.Log($"Map {mapNumber} vẫn bị khóa!");
        }
    }

    /// <summary>
    /// Gọi hàm này sau khi unlock map mới để cập nhật UI
    /// </summary>
    public void OnMapUnlocked(int mapNumber)
    {
        UpdateMapUI(mapNumber);
        
        // Phát hiệu ứng hoặc âm thanh khi unlock (tuỳ chọn)
        PlayUnlockAnimation(mapNumber);
    }

    /// <summary>
    /// Hiệu ứng khi unlock map (tuỳ chọn)
    /// </summary>
    private void PlayUnlockAnimation(int mapNumber)
    {
        int index = mapNumber - 1;
        if (lockIconUI[index] != null)
        {
            StartCoroutine(FadeOutLock(lockIconUI[index]));
        }
    }

    private System.Collections.IEnumerator FadeOutLock(GameObject lockIcon)
    {
        CanvasGroup canvasGroup = lockIcon.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = lockIcon.AddComponent<CanvasGroup>();
        }

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        lockIcon.SetActive(false);
    }
}
