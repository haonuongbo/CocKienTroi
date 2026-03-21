using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class WinScreenUI : MonoBehaviour
{
    // Optional: keep this UI alive across scene loads so stored rank data / references don't get lost.
    // If you have multiple RaceRankUI instances across scenes, only the first will persist.
    static RaceRankUI instance;

    [Header("References")]
    public RaceRankManager positionCalculator;

    [Header("Cards & Rank Slots")]
    public Transform[] nameCards;     // 5 cards (each has Image + TMP_Text)
    public Transform[] rankSlots;     // 5 placeholders (1st → 5th)

    // Lưu tên object con trong mỗi rank slot (dùng khi cần truyền dữ liệu qua scene khác)
    public List<string> childObjectNames = new List<string>();

    // Cache của thứ tự mới nhất (dùng để tránh update không cần thiết mỗi frame)
    int[] lastRankedCarIndex;

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
            card.localScale = Vector3.one;
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

            Transform card = nameCards[carIndex];
            if (card == null)
            {
                winSprites.Add(null);
                continue;
            }

            var img = card.GetComponentInChildren<Image>();
            if (img == null || img.sprite == null)
            {
                winSprites.Add(null);
                continue;
            }

            string spriteName = img.sprite.name;
            string winName = spriteName.Replace("1", "2");
            winSprites.Add(Resources.Load<Sprite>(winName));
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

        public static Sprite GetWinSprite(int rank)
        {
            if (WinSprites == null || rank < 1 || rank > WinSprites.Count)
                return null;

            return WinSprites[rank - 1];
        }
    }
}
