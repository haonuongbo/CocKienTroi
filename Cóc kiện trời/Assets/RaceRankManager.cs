using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RaceRankManager : MonoBehaviour
{
    [Header("Racers (5)")]
    public Transform[] racers;          // size = 5
    public string[] racerNames;         // size = 5

    [Header("Name Cards (1st → 5th)")]
    public Transform[] nameCards;       // size = 5
    // each nameCard:
    // ├─ Image (icon/background)
    // └─ Text (TMP_Text)

    void Update()
    {
        UpdateRanking();
    }

    void UpdateRanking()
    {
        // sort racers by forward progress (top-down vertical)
        System.Array.Sort(racers, (a, b) =>
            b.position.y.CompareTo(a.position.y));

        for (int rank = 0; rank < racers.Length; rank++)
        {
            Transform card = nameCards[rank];

            TMP_Text text = card.GetComponentInChildren<TMP_Text>();
            Image image = card.GetComponentInChildren<Image>();

            int racerIndex = GetOriginalIndex(racers[rank]);

            // update text
            if (text != null)
                text.text = (rank + 1) + ". " + racerNames[racerIndex];

            // optional: visual highlight for top ranks
            if (image != null)
            {
                image.color = rank == 0 ? Color.yellow : Color.white;
            }
        }
    }

    int GetOriginalIndex(Transform racer)
    {
        for (int i = 0; i < racers.Length; i++)
        {
            if (racers[i] == racer)
                return i;
        }
        return 0;
    }
}
