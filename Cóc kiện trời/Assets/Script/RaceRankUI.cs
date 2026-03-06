using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class RaceRankUI : MonoBehaviour
{
    [Header("References")]
    public RaceRankManager positionCalculator;

    [Header("Cards & Rank Slots")]
    public Transform[] nameCards;     // 5 cards (each has Image + TMP_Text)
    public Transform[] rankSlots;     // 5 placeholders (1st → 5th)

    // internal state used to avoid flooding the carrier every frame
    bool hasSentRankings;

    void Update()
    {
        UpdateCardPositions();
    }

    void UpdateCardPositions()
    {
        for (int rank = 0; rank < rankSlots.Length; rank++)
        {
            int carIndex = positionCalculator.rankedCarIndex[rank];
            Transform card = nameCards[carIndex];

            // move card to correct rank slot
            card.SetParent(rankSlots[rank], false);
            card.localPosition = Vector3.zero;
            card.localRotation = Quaternion.identity;
            card.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Call this when the race has finished to remember which cars occupied the
    /// different positions.  The parking scene can then query <see cref="RankCarrier" />
    /// for the appropriate sprites when showing the victory screen.
    /// </summary>
    public void SendRankingsToCarrier()
    {
        if (hasSentRankings || RankCarrier.Instance == null)
            return;

        var list = new List<RankCarrier.RankEntry>();
        for (int i = 0; i < rankSlots.Length; i++)
        {
            int carIndex = positionCalculator.rankedCarIndex[i];
            Transform card = nameCards[carIndex];
            GameObject cardObj = card != null ? card.gameObject : null;

            // rank stored as 1-based for readability
            list.Add(new RankCarrier.RankEntry(i + 1, cardObj));
        }

        RankCarrier.Instance.StoreRankings(list);
        hasSentRankings = true;
    }
}
