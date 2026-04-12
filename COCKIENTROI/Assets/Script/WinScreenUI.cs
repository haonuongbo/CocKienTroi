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

    [Header("Win Scene Sprites")]
    public Sprite[] winCharacterSprites; // Kéo thả 5 hình nhân vật Win (Cáo, Cóc...)
    public Sprite[] winNameSprites;      // Kéo thả 5 hình CHỮ TÊN (Cáo, Cóc...) dùng cho Top 1
    
    [Header("Win Canvas Element")]
    public Image winnerNameImageDisplay; // Kéo thả cục ảnh WinnerName vào đây!

    [Header("Floating Animation")]
    [SerializeField] private bool enableFloatingAnimation = true;
    [SerializeField] private int floatingTopCount = 3;
    [SerializeField] private float floatingAmplitude = 8f;
    [SerializeField] private float floatingFrequency = 1.8f;

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

            // Đổi mặt hình nhân vật cho Win Scene (Vì Win Scene dùng hình nhân vật lớn thay vì hình Avatar nhỏ)
            if (winCharacterSprites != null && carIndex < winCharacterSprites.Length && winCharacterSprites[carIndex] != null)
            {
                // DỌN RÁC: Ẩn TOÀN BỘ các khung viền, avatar con, và chữ của thẻ Race Rank cũ
                Image[] allImages = card.GetComponentsInChildren<Image>(true);
                for (int j = 0; j < allImages.Length; j++)
                {
                    allImages[j].enabled = false;
                    
                    // Phá luôn Mask nếu có để không bị phạt cắt tròn ảnh
                    Mask mask = allImages[j].GetComponent<Mask>();
                    if (mask != null) Destroy(mask);
                }

                // DÙNG DUY NHẤT 1 BỨC TRANH LÀ ẢNH GỐC
                if (allImages.Length > 0)
                {
                    allImages[0].enabled = true;
                    allImages[0].sprite = winCharacterSprites[carIndex];
                    allImages[0].color = Color.white;
                }

                // Dọn luôn mấy cái chữ tên nhỏ xíu ở dưới xe (vì đã có tên Winner Name bự rồi)
                TMP_Text[] allTexts = card.GetComponentsInChildren<TMP_Text>(true);
                for (int j = 0; j < allTexts.Length; j++)
                {
                    allTexts[j].enabled = false;
                }
            }

            card.SetParent(slot, false);
            card.localPosition = Vector3.zero;
            card.localRotation = Quaternion.identity;
            card.localScale = Vector3.one;

            ConfigureFloatingAnimation(card, enableFloatingAnimation && rank < floatingTopCount);
        }

        if (lastRankedCarIndex != null && current.Length == lastRankedCarIndex.Length)
            System.Array.Copy(current, lastRankedCarIndex, current.Length);

        // --- Cập nhật Win Name ---
        if (winnerNameImageDisplay != null && winNameSprites != null && current.Length > 0)
        {
            int top1CarIndex = current[0];
            if (top1CarIndex >= 0 && top1CarIndex < winNameSprites.Length && winNameSprites[top1CarIndex] != null)
            {
                winnerNameImageDisplay.sprite = winNameSprites[top1CarIndex];
                // KHÔNG call SetNativeSize() nữa để giữ nguyên khung hình Width/Height gốc do user scale tay!
            }
        }
    }

    void ConfigureFloatingAnimation(Transform card, bool shouldFloat)
    {
        if (card == null)
            return;

        FloatingChar floating = card.GetComponent<FloatingChar>();

        if (!shouldFloat)
        {
            if (floating != null)
                floating.isFloating = false;
            return;
        }

        if (floating == null)
            floating = card.gameObject.AddComponent<FloatingChar>();

        floating.charPreviewImage = card.GetComponent<Image>();
        floating.amplitude = floatingAmplitude;
        floating.frequency = floatingFrequency;
        floating.isFloating = true;
        floating.ResetStartPosition();
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
