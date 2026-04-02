using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class RaceRankUI : MonoBehaviour
{
    // Optional: keep this UI alive across scene loads so stored rank data / references don't get lost.
    // If you have multiple RaceRankUI instances across scenes, only the first will persist.
    static RaceRankUI instance;

    [Header("References")]
    public RaceRankManager positionCalculator;

    [Header("Cards & Rank Slots")]
    public Transform[] nameCards;     // 5 cards (each has Image + TMP_Text)
    public Transform[] rankSlots;     // 5 placeholders (1st → 5th)

    [Header("UI Styling")]
    public float top1FontSize = 38f;
    public float normalFontSize = 28f; // Bạn có thể chỉnh lại số này trong Inspector nếu chữ nhỏ quá

    // Lưu tên object con trong mỗi rank slot (dùng khi cần truyền dữ liệu qua scene khác)
    public List<string> childObjectNames = new List<string>();

    // Cache của thứ tự mới nhất (dùng để tránh update không cần thiết mỗi frame)
    int[] lastRankedCarIndex;
    
    // Lưu lại vị trí Y gốc của các đoạn text
    float[] originalTextY;

    void Awake()
    {
    }

    void Start()
    {
        InitializeCache();
        RefreshRankDisplay();
    }

    void Update()
    {
        if (NeedsRefresh())
            RefreshRankDisplay();
    }

    void InitializeCache()
    {
        if (rankSlots != null)
            lastRankedCarIndex = new int[rankSlots.Length];
            
        if (nameCards != null)
        {
            originalTextY = new float[nameCards.Length];
            for (int i = 0; i < nameCards.Length; i++)
            {
                if (nameCards[i] != null)
                {
                    TMP_Text txt = nameCards[i].GetComponentInChildren<TMP_Text>();
                    if (txt != null)
                    {
                        RectTransform textRect = txt.GetComponent<RectTransform>();
                        if (textRect != null) originalTextY[i] = textRect.anchoredPosition.y;
                    }
                }
            }
        }
    }

    bool NeedsRefresh()
    {
        if (positionCalculator == null || rankSlots == null || nameCards == null)
            return false;

        var current = positionCalculator.rankedCarIndex;
        if (current == null)
            return false;

        if (lastRankedCarIndex == null || current.Length != lastRankedCarIndex.Length)
            return true;

        for (int i = 0; i < lastRankedCarIndex.Length; i++)
        {
            if (lastRankedCarIndex[i] != current[i])
                return true;
        }

        return false;
    }

    void RefreshRankDisplay()
    {
        UpdateCardPositions();
        UpdateChildObjectNamesFromSlots();
        CacheRankData();
    }

    /// <summary>
    /// Call this when you want to capture the latest ranking data for use in another scene.
    /// </summary>
    public void CaptureRankData()
    {
        RefreshRankDisplay();
    }

    void UpdateCardPositions()
    {
        if (positionCalculator == null || rankSlots == null || nameCards == null)
            return;

        var current = positionCalculator.rankedCarIndex;
        if (current == null)
            return;

        int count = Mathf.Min(rankSlots.Length, current.Length);
        for (int rank = 0; rank < count; rank++)
        {
            int carIndex = current[rank];
            if (carIndex < 0 || carIndex >= nameCards.Length)
                continue;

            Transform card = nameCards[carIndex];
            if (card == null)
                continue;

            Transform slot = rankSlots[rank];
            if (slot == null)
                continue;

            card.SetParent(slot, false);
            card.localPosition = Vector3.zero;
            card.localRotation = Quaternion.identity;
            
            // Xử lý phóng to và cỡ chữ cho Top 1
            TMP_Text nameText = card.GetComponentInChildren<TMP_Text>();

            if (rank == 0)
            {
                // Phóng to thẻ Top 1 (bạn có thể điều chỉnh số 1.3f to nhỏ tùy ý)
                card.localScale = new Vector3(1.3f, 1.3f, 1f); 
                if (nameText != null) 
                {
                    nameText.fontSize = top1FontSize;
                }
            }
            else
            {
                // Các thẻ còn lại giữ nguyên kích thước gốc
                card.localScale = Vector3.one;
                if (nameText != null) 
                {
                    nameText.fontSize = normalFontSize;
                }
            }
        }

        if (lastRankedCarIndex != null && current.Length == lastRankedCarIndex.Length)
            System.Array.Copy(current, lastRankedCarIndex, current.Length);
    }

    void CacheRankData()
    {
        RaceRankData.ChildObjectNames = new List<string>(childObjectNames);
        RaceRankData.WinSprites = BuildWinSprites();
    }

    List<Sprite> BuildWinSprites()
    {
        var winSprites = new List<Sprite>();

        if (positionCalculator == null || nameCards == null)
            return winSprites;

        var current = positionCalculator.rankedCarIndex;
        if (current == null)
            return winSprites;

        int count = Mathf.Min(rankSlots != null ? rankSlots.Length : 0, current.Length);
        for (int i = 0; i < count; i++)
        {
            int carIndex = current[i];
            if (carIndex < 0 || carIndex >= nameCards.Length)
            {
                winSprites.Add(null);
                continue;
            }

            // Mượn đỡ tấm hình trên nameCard
            Transform card = nameCards[carIndex];
            if (card != null)
            {
                var img = card.GetComponentInChildren<Image>();
                winSprites.Add(img != null ? img.sprite : null);
            }
            else
            {
                winSprites.Add(null);
            }
        }

        return winSprites;
    }

    /// <summary>
    /// Lấy parent object (rank slot) ở vị trí rank (0-based).
    /// </summary>
    public GameObject GetParentFromSlot(int rank)
    {
        if (rankSlots == null || rank < 0 || rank >= rankSlots.Length)
            return null;

        return rankSlots[rank] != null ? rankSlots[rank].gameObject : null;
    }

    /// <summary>
    /// Cập nhật danh sách tên object con (nếu tồn tại) cho mỗi rank slot.
    /// </summary>
    public void UpdateChildObjectNamesFromSlots()
    {
        childObjectNames.Clear();

        if (rankSlots == null)
            return;

        for (int i = 0; i < rankSlots.Length; i++)
        {
            var slot = rankSlots[i];
            string childName = null;

            if (slot != null && slot.childCount > 0)
            {
                childName = slot.GetChild(0).gameObject.name;
            }

            childObjectNames.Add(childName);
        }
    }

    public static class RaceRankData
    {
        public static List<string> ChildObjectNames { get; set; } = new List<string>();
        public static List<Sprite> WinSprites { get; set; } = new List<Sprite>();
        public static Sprite WinnerNameSprite { get; set; } // Ảnh chứa chữ của Top 1

        public static Sprite GetWinSprite(int rank)
        {
            if (WinSprites == null || rank < 1 || rank > WinSprites.Count)
                return null;

            return WinSprites[rank - 1];
        }
    }
}
