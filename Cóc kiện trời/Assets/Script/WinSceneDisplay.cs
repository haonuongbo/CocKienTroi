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
        if (RankCarrier.Instance == null)
        {
            Debug.LogError("RankCarrier singleton not found!");
            return;
        }

        // fetch win sprites for positions 1, 2, 3
        Sprite firstSprite = RankCarrier.Instance.GetWinSprite(1);
        Sprite secondSprite = RankCarrier.Instance.GetWinSprite(2);
        Sprite thirdSprite = RankCarrier.Instance.GetWinSprite(3);

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
    }
}
