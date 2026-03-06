using UnityEngine;
using System.Collections.Generic;

public class RankCarrier : MonoBehaviour
{
    public static RankCarrier Instance { get; private set; }

    [System.Serializable]
    public class RankEntry
    {
        [SerializeField] public int rank;
        [SerializeField] public GameObject gameObject;
        [SerializeField] public Sprite winSprite; // optional, can be null

        // useful ctor if you want to build the list in code
        public RankEntry(int r, GameObject obj, Sprite win = null)
        {
            rank = r;
            gameObject = obj;
            winSprite = win;
        }
    }

    [SerializeField] private List<RankEntry> Rankings = new List<RankEntry>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called from the race scene to remember which sprites were used for each position.
    /// You can provide both a racingSprite and a winSprite; if winSprite is left null
    /// the helper GetWinSprite will attempt to infer a variant by name.
    /// </summary>
    public void StoreRankings(List<RankEntry> rankList)
    {
        Rankings = rankList;
    }

    /// <summary>
    /// Walks the first <see cref="RaceRankUI" /> found in the scene and copies
    /// its card gameObjects into the internal ranking list.  This mirrors the logic
    /// used by <c>RaceRankUI.SendRankingsToCarrier</c> so that the carrier can
    /// pull data directly instead of requiring an external caller.
    /// </summary>
    public void PullFromRaceRankUI()
    {
        var ui = FindObjectOfType<RaceRankUI>();
        if (ui == null)
            return;

        var list = new List<RankEntry>();
        int count = ui.rankSlots != null ? ui.rankSlots.Length : 0;
        for (int i = 0; i < count; i++)
        {
            int carIndex = ui.positionCalculator.rankedCarIndex[i];
            Transform card = ui.nameCards[carIndex];
            GameObject cardObj = card != null ? card.gameObject : null;

            list.Add(new RankEntry(i + 1, cardObj));
        }

        Rankings = list;
    }

    /// <summary>
    /// If enabled, attempt to pull the rankings from the RaceRankUI in Start().
    /// This is useful when the carrier is created in the race scene and you want
    /// it to self-initialize automatically.
    /// </summary>
    public bool autoPullFromUI = false;

    private void Start()
    {
        if (autoPullFromUI)
            PullFromRaceRankUI();
    }

    public Sprite GetRacingSprite(int rank)
    {
        var e = Rankings.Find(entry => entry.rank == rank);
        if (e == null || e.gameObject == null)
            return null;

        var img = e.gameObject.GetComponentInChildren<UnityEngine.UI.Image>();
        return img != null ? img.sprite : null;
    }

    /// <summary>
    /// Returns the sprite that should be displayed in the victory/win scene for
    /// the given position.  If the entry contains an explicit <see cref="winSprite" />
    /// that will be returned.  Otherwise we try to load a variant from Resources
    /// by replacing "1" with "2" in the original sprite's name (e.g. "ferrari1" -> "ferrari2").
    /// Adjust the naming logic to suit your project.
    /// </summary>
    public Sprite GetWinSprite(int rank)
    {
        var e = Rankings.Find(entry => entry.rank == rank);
        if (e == null)
            return null;

        if (e.winSprite != null)
            return e.winSprite;

        if (e.gameObject != null)
        {
            var img = e.gameObject.GetComponentInChildren<UnityEngine.UI.Image>();
            if (img != null && img.sprite != null)
            {
                string spriteName = img.sprite.name;
                string winName = spriteName.Replace("1", "2");
                return Resources.Load<Sprite>(winName);
            }
        }

        return null;
    }
}