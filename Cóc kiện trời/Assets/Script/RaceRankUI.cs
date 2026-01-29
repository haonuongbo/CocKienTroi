using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RaceRankUI : MonoBehaviour
{
    [Header("References")]
    public RaceRankManager positionCalculator;

    [Header("Cards & Rank Slots")]
    public Transform[] nameCards;     // 5 cards (each has Image + TMP_Text)
    public Transform[] rankSlots;     // 5 placeholders (1st → 5th)

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
}
