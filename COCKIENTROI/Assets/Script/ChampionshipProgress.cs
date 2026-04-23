using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChampionshipProgress
{
    private const string SelectedCharacterIndexKey = "Championship.SelectedCharacterIndex";
    private const string SelectedCharacterNameKey = "Championship.SelectedCharacterName";

    private static readonly string[] RaceMaps = { "M1", "M2", "M3" };

    private static string BuildMapRankKey(string mapName)
    {
        return "Championship.Rank." + mapName;
    }

    public static void ResetCampaign()
    {
        for (int i = 0; i < RaceMaps.Length; i++)
        {
            PlayerPrefs.SetInt(BuildMapRankKey(RaceMaps[i]), -1);
        }

        PlayerPrefs.Save();
    }

    public static void SetSelectedCharacter(int selectedIndex, string selectedName)
    {
        PlayerPrefs.SetInt(SelectedCharacterIndexKey, selectedIndex);
        PlayerPrefs.SetString(SelectedCharacterNameKey, selectedName ?? string.Empty);
        PlayerPrefs.Save();
    }

    public static int GetSelectedCharacterIndex()
    {
        return PlayerPrefs.GetInt(SelectedCharacterIndexKey, 0);
    }

    public static string GetSelectedCharacterName()
    {
        return PlayerPrefs.GetString(SelectedCharacterNameKey, string.Empty);
    }

    public static void RecordRankForCurrentScene(int rank)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        RecordRank(sceneName, rank);
    }

    public static void RecordRank(string mapName, int rank)
    {
        if (string.IsNullOrWhiteSpace(mapName))
            return;

        int clampedRank = Mathf.Clamp(rank, 1, 5);

        for (int i = 0; i < RaceMaps.Length; i++)
        {
            if (RaceMaps[i] != mapName)
                continue;

            PlayerPrefs.SetInt(BuildMapRankKey(mapName), clampedRank);
            PlayerPrefs.Save();
            return;
        }
    }

    public static bool TryGetMapRank(string mapName, out int rank)
    {
        rank = -1;

        if (string.IsNullOrWhiteSpace(mapName))
            return false;

        int stored = PlayerPrefs.GetInt(BuildMapRankKey(mapName), -1);
        if (stored < 1 || stored > 5)
            return false;

        rank = stored;
        return true;
    }

    public static List<int> GetRecordedRanks()
    {
        List<int> ranks = new List<int>();

        for (int i = 0; i < RaceMaps.Length; i++)
        {
            if (TryGetMapRank(RaceMaps[i], out int rank))
                ranks.Add(rank);
        }

        return ranks;
    }

    public static int GetRoundedAverageRank()
    {
        List<int> ranks = GetRecordedRanks();
        if (ranks.Count == 0)
            return 5;

        float sum = 0f;
        for (int i = 0; i < ranks.Count; i++)
            sum += ranks[i];

        int rounded = Mathf.RoundToInt(sum / ranks.Count);
        return Mathf.Clamp(rounded, 1, 5);
    }

    public static int GetStarsFromRoundedRank(int roundedRank)
    {
        int clamped = Mathf.Clamp(roundedRank, 1, 5);
        return Mathf.Clamp(6 - clamped, 1, 5);
    }
}