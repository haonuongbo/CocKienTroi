using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalWinManager : MonoBehaviour
{
    [Header("---- KÉO 5 HÌNH VÀO MỖI DÒNG DƯỚI ĐÂY ----")]
    [Tooltip("5 Hình nhân vật tương ứng với ID Xe lúc chọn (Cáo, Cóc, Cọp...)")]
    public Sprite[] characterSprites;

    [Tooltip("5 Hình chữ Title tương ứng với ID Xe (Hảo Tướng Bố...)")]
    public Sprite[] titleSprites;

    [Tooltip("5 Hình Đánh Giá Sao (1 Sao -> 5 Sao)")]
    public Sprite[] starSprites;

    [Tooltip("5 Hình Ruy Băng Hạng (Hạng 1 -> Hạng 5)")]
    public Sprite[] rankBadgeSprites;

    [Header("---- THÀNH PHẦN GIAO DIỆN ----")]
    // Tự động tìm từ Hierarchy nhưng cứ để public cho bạn dễ sửa
    public Image playerAvatarImage;
    public Image titleImage;
    public Image starsImage;
    public Image rankBadgeImage;
    public Button retryButton;
    public Button menuButton;

    void Start()
    {
        AutoFindUI();
        CalculateAndShowResult();
        SetupButtons();
    }

    void AutoFindUI()
    {
        if (playerAvatarImage == null)
        {
            GameObject go = GameObject.Find("PlayerCharacter");
            if (go != null) playerAvatarImage = go.GetComponent<Image>();
        }
        if (titleImage == null)
        {
            GameObject go = GameObject.Find("Title");
            if (go != null) titleImage = go.GetComponent<Image>();
        }
        if (starsImage == null)
        {
            GameObject go = GameObject.Find("Stars");
            if (go != null) starsImage = go.GetComponent<Image>();
        }
        if (rankBadgeImage == null)
        {
            GameObject go = GameObject.Find("RankBadge");
            if (go != null) rankBadgeImage = go.GetComponent<Image>();
        }
        if (retryButton == null)
        {
            GameObject go = GameObject.Find("RetryButton");
            if (go != null) retryButton = go.GetComponent<Button>();
        }
        if (menuButton == null)
        {
            GameObject go = GameObject.Find("MenuButton");
            if (go != null) menuButton = go.GetComponent<Button>();
        }
    }

    void CalculateAndShowResult()
    {
        // 1. Áp dụng hiệu ứng nhún nhảy cho Player Avatar
        if (playerAvatarImage != null)
        {
            FloatingChar floating = playerAvatarImage.gameObject.GetComponent<FloatingChar>();
            if (floating == null)
            {
                floating = playerAvatarImage.gameObject.AddComponent<FloatingChar>();
            }
            floating.amplitude = 15f; 
            floating.frequency = 1.2f;
            floating.isFloating = true;
        }

        // 2. Lấy ID nhân vật đã chọn để hiện hình
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);

        if (playerAvatarImage != null && characterSprites != null && selectedIndex < characterSprites.Length)
        {
            playerAvatarImage.sprite = characterSprites[selectedIndex];
        }

        if (titleImage != null && titleSprites != null && selectedIndex < titleSprites.Length)
        {
            titleImage.sprite = titleSprites[selectedIndex];
        }

        // 3. Tính toán 5 Ngôi sao dựa vào Hạng (Trung bình cộng)
        // Lấy 0 để đếm xem Map nào đã thực sự được chơi (để khỏi cộng oan hạng 1 nếu người chơi đi tắt)
        int rank1 = PlayerPrefs.GetInt("PlayerRank_Map1", 0); 
        int rank2 = PlayerPrefs.GetInt("PlayerRank_Map2", 0);
        int rank3 = PlayerPrefs.GetInt("PlayerRank_Map3", 0);

        int sumRank = 0;
        int mapPlayed = 0;

        if (rank1 > 0) { sumRank += rank1; mapPlayed++; }
        if (rank2 > 0) { sumRank += rank2; mapPlayed++; }
        if (rank3 > 0) { sumRank += rank3; mapPlayed++; }

        int averageRank = 1; // Mặc định nếu không chơi gì cả
        if (mapPlayed > 0)
        {
            averageRank = Mathf.RoundToInt((float)sumRank / mapPlayed);
        }
        
        // GIẢI QUYẾT BADGE: Hạng trung bình là bao nhiêu thì hiện Badge số đó
        // (Khóa index lại để chắc chắn từ 1->5)
        int badgeIndex = Mathf.Clamp(averageRank, 1, 5) - 1;
        if (rankBadgeImage != null && rankBadgeSprites != null && rankBadgeSprites.Length >= 5)
        {
            rankBadgeImage.sprite = rankBadgeSprites[badgeIndex];
        }

        // Quy tắc mộc: Hạng TB = 1 -> 5 sao. Hạng TB = 5 -> 1 sao.
        int starCount = 6 - averageRank;
        
        // Khóa lại tránh lỗi Index
        starCount = Mathf.Clamp(starCount, 1, 5); 

        if (starsImage != null && starSprites != null && starSprites.Length >= 5)
        {
            // starSprites[0] là hình 1 sao, starSprites[4] là hình 5 sao
            starsImage.sprite = starSprites[starCount - 1];
        }
    }

    void SetupButtons()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(() => {
                ClearRankHistory();
                SceneManager.LoadScene("CHARACTER SELECTION");
            });
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => {
                ClearRankHistory();
                SceneManager.LoadScene("SPLASH ART");
            });
        }
    }

    void ClearRankHistory()
    {
        PlayerPrefs.DeleteKey("PlayerRank_Map1");
        PlayerPrefs.DeleteKey("PlayerRank_Map2");
        PlayerPrefs.DeleteKey("PlayerRank_Map3");
        PlayerPrefs.Save();
    }
}
