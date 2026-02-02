using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceWinHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MapUIController mapUIController;
    [SerializeField] private Button nextMapButton;
    [SerializeField] private Button homeButton;
    
    [Header("Win Screen")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI finishTimeText;

    private int currentMapNumber = 1;

    private void Start()
    {
        if (nextMapButton != null)
            nextMapButton.onClick.AddListener(GoToNextMap);
        
        if (homeButton != null)
            homeButton.onClick.AddListener(GoHome);
    }

    /// <summary>
    /// Gọi hàm này khi người chơi thắng một map
    /// </summary>
    public void OnRaceWin(float finishTime)
    {
        // Lưu thời gian hoàn thành (tuỳ chọn)
        // PlayerPrefs.SetFloat($"MapTime_{currentMapNumber}", finishTime);

        // Mở khóa map tiếp theo
        MapManager.Instance.WinMap(currentMapNumber);
        
        // Cập nhật giao diện
        if (mapUIController != null)
        {
            mapUIController.OnMapWon(currentMapNumber);
        }

        // Hiển thị màn hình thắng
        ShowWinScreen(finishTime);
    }

    private void ShowWinScreen(float finishTime)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (finishTimeText != null)
        {
            int minutes = Mathf.FloorToInt(finishTime / 60f);
            int seconds = Mathf.FloorToInt(finishTime % 60f);
            int milliseconds = Mathf.FloorToInt((finishTime * 100f) % 100f);
            finishTimeText.text = $"Time: {minutes:00}:{seconds:00}:{milliseconds:00}";
        }
    }

    public void SetCurrentMap(int mapNumber)
    {
        currentMapNumber = mapNumber;
    }

    private void GoToNextMap()
    {
        if (currentMapNumber < 3) // Nếu chưa phải map cuối
        {
            // TODO: Load map tiếp theo
            // SceneManager.LoadScene($"Map_{currentMapNumber + 1}");
            Debug.Log($"Loading Map {currentMapNumber + 1}");
        }
    }

    private void GoHome()
    {
        // TODO: Load scene chọn map hoặc menu chính
        // SceneManager.LoadScene("MapSelection");
        Debug.Log("Going back to map selection");
    }
}
