using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Map Preview")]
    [SerializeField] private Image mapPreviewImage;        // Ảnh preview map lớn

    [Header("Navigation Buttons")]
    [SerializeField] private Button prevButton;   // Nút prev (<)
    [SerializeField] private Button nextButton;   // Nút next (>)
    [SerializeField] private Button menuButton;   // Nút MENU
    [SerializeField] private Button playButton;   // Nút DUA/RUN

    [Header("Lock Icon")]
    [SerializeField] private GameObject lockIcon; // Icon khóa

    [Header("Map Data")]
    [SerializeField] private Sprite[] mapPreviewSprites; // Sprite ảnh các map
    [SerializeField] private string[] mapNames;         // Tên các map
    [SerializeField] private string[] mapSceneNames;     // Tên scene các map

    [Header("Colors")]
    [SerializeField] private Color unlockedTextColor = Color.white;
    [SerializeField] private Color lockedTextColor = Color.gray;

    private int currentMapIndex = 0;
    private int totalMaps;

    private void Start()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager không tìm thấy!");
            return;
        }

        // Setup buttons
        if (prevButton != null) prevButton.onClick.AddListener(ShowPreviousMap);
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextMap);
        if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);
        if (playButton != null) playButton.onClick.AddListener(PlaySelectedMap);

        // Lấy tổng số map từ mảng inspector
        totalMaps = mapPreviewSprites != null ? mapPreviewSprites.Length : 0;

        // Hiển thị map đầu tiên
        currentMapIndex = 0;
        RefreshMapPreview();
    }

    /// <summary>
    /// Cập nhật preview map hiện tại
    /// </summary>
    private void RefreshMapPreview()
    {
        int mapNumber = currentMapIndex + 1;
        bool isUnlocked = MapManager.Instance.IsMapUnlocked(mapNumber);

        // Cập nhật ảnh preview
        if (mapPreviewImage != null && currentMapIndex < mapPreviewSprites.Length)
        {
            mapPreviewImage.sprite = mapPreviewSprites[currentMapIndex];
            mapPreviewImage.color = isUnlocked ? Color.white : Color.gray;
        }

        // Cập nhật icon khóa
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        // Cập nhật trạng thái nút play
        if (playButton != null)
        {
            playButton.interactable = isUnlocked;
        }
    }

    private void ShowPreviousMap()
    {
        currentMapIndex--;
        if (currentMapIndex < 0)
            currentMapIndex = totalMaps - 1;
        
        RefreshMapPreview();
    }

    private void ShowNextMap()
    {
        currentMapIndex++;
        if (currentMapIndex >= totalMaps)
            currentMapIndex = 0;
        
        RefreshMapPreview();
    }

    private void PlaySelectedMap()
    {
        int mapNumber = currentMapIndex + 1;
        if (MapManager.Instance.IsMapUnlocked(mapNumber))
        {
            Debug.Log($"Đang tải {mapNames[currentMapIndex]}...");
            // Lưu scene name vào PlayerPrefs để LoadingScene load
            PlayerPrefs.SetString("NextSceneToLoad", mapSceneNames[currentMapIndex]);
            // Load loading scene
            SceneManager.LoadScene("Loading");
        }
    }

    private void GoToMenu()
    {
        Debug.Log("Quay về menu...");
        // Load menu scene
        SceneManager.LoadScene("MenuScene");
    }

    /// <summary>
    /// Gọi khi unlock map mới
    /// </summary>
    public void OnMapUnlocked(int mapNumber)
    {
        RefreshMapPreview();
    }
}
