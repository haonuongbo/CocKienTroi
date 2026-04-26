using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the Top 3 racers' win-scene images in the victory screen.
/// This script pulls ranking data from RankCarrier (populated by RaceRankUI)
/// and assigns the appropriate win sprites to the three image placeholders.
/// </summary>
public class WinSceneDisplay : MonoBehaviour
{
    [Header("Top 3 Place Holder Images")]
    [SerializeField] private Image firstPlaceImage;   // 1st place racer image
    [SerializeField] private Image secondPlaceImage;  // 2nd place racer image
    [SerializeField] private Image thirdPlaceImage;   // 3rd place racer image
    [SerializeField] private Image winnerNameImage;   // Ảnh chữ vinh danh Top 1 (WinnerName)

    void Start()
    {
        DisplayTopThreeRacers();
    }

    /// <summary>
    /// Queries RankCarrier for the top 3 positions and displays their
    /// win-scene sprites (e.g. "ferrari2.png" for the racer who had "ferrari1.png" in race).
    /// </summary>
    void DisplayTopThreeRacers()
    {
        // fetch win sprites for positions 1, 2, 3 (captured by RaceRankUI)
        Sprite firstSprite = RaceRankUI.RaceRankData.GetWinSprite(1);
        Sprite secondSprite = RaceRankUI.RaceRankData.GetWinSprite(2);
        Sprite thirdSprite = RaceRankUI.RaceRankData.GetWinSprite(3);

        // Nạp ảnh chữ Winner Name (nếu có)
        Sprite winnerNameSprite = RaceRankUI.RaceRankData.WinnerNameSprite;
        if (winnerNameImage != null && winnerNameSprite != null)
        {
            winnerNameImage.sprite = winnerNameSprite;
            winnerNameImage.SetNativeSize(); // Gọi hàm này để tỷ lệ chữ không bị méo lệch
        }

        // assign to placeholders
        if (firstPlaceImage != null && firstSprite != null)
            firstPlaceImage.sprite = firstSprite;
        else if (firstPlaceImage != null)
            Debug.LogWarning("No sprite found for 1st place or firstPlaceImage is not assigned!");

        if (secondPlaceImage != null && secondSprite != null)
            secondPlaceImage.sprite = secondSprite;
        else if (secondPlaceImage != null)
            Debug.LogWarning("No sprite found for 2nd place or secondPlaceImage is not assigned!");

        if (thirdPlaceImage != null && thirdSprite != null)
            thirdPlaceImage.sprite = thirdSprite;
        else if (thirdPlaceImage != null)
            Debug.LogWarning("No sprite found for 3rd place or thirdPlaceImage is not assigned!");

        // --- MỚI: Tùy chỉnh tỷ lệ kích thước (Scale) cho Top 1 to ra, Top 2 và 3 nhỏ lại ---
        if (firstPlaceImage != null) firstPlaceImage.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        if (secondPlaceImage != null) secondPlaceImage.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        if (thirdPlaceImage != null) thirdPlaceImage.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        // --- MỚI: Thêm nhún nhún cho Top 1, 2, 3 ---
        AddFloatingEffect(firstPlaceImage);
        AddFloatingEffect(secondPlaceImage);
        AddFloatingEffect(thirdPlaceImage);
    }

    /// <summary>
    /// Thêm component FloatingChar để nhân vật nhún thả ga như ở màn hình chọn nhân vật
    /// </summary>
    private void AddFloatingEffect(Image img)
    {
        if (img == null || img.gameObject == null) return;
        
        FloatingChar floating = img.gameObject.GetComponent<FloatingChar>();
        if (floating == null)
        {
            floating = img.gameObject.AddComponent<FloatingChar>();
        }
        
        // Set tham số nhún tương tự CharacterSelection
        floating.amplitude = 15f; 
        floating.frequency = 1.2f;
        floating.isFloating = true;
    }
}
