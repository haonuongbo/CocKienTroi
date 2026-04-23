using UnityEngine;

public class RaceRankManager : MonoBehaviour
{
    [Header("Cars (fixed = 5)")]
    public RaceProgressTracker[] trackers; // size = 5

    [Header("Runtime ranking result")]
    public int[] rankedCarIndex = new int[5]; // 0 = 1st place

    [Header("Lock Status")]
    public bool isPlayerFinished = false; // Khi player đã finish thì dừng cập nhật để không bị tuột rank
    public int playerFinalRankIndex = -1;

    void Update()
    {
        if (trackers == null || trackers.Length != 5) return;
        
        // NẾU NGƯỜI CHƠI ĐÃ XONG VÀ ĐANG THƯỞNG THỨC 5S PHÁO BÔNG -> ĐÓNG BĂNG BẢNG XẾP HẠNG!
        if (isPlayerFinished) return;

        CalculateRanking();
    }

    void CalculateRanking()
    {
        for (int i = 0; i < 5; i++)
            rankedCarIndex[i] = i;

        System.Array.Sort(rankedCarIndex, (a, b) =>
        {
            RaceProgressTracker A = trackers[a];
            RaceProgressTracker B = trackers[b];

            if (A == null && B == null) return 0;
            if (A == null) return 1;
            if (B == null) return -1;

            // 1. Lap (higher = better)
            if (A.CurrentLap != B.CurrentLap)
                return B.CurrentLap.CompareTo(A.CurrentLap);

            // 2. Checkpoints passed (higher = better)
            if (A.CheckpointsPassed != B.CheckpointsPassed)
                return B.CheckpointsPassed.CompareTo(A.CheckpointsPassed);

            // 3. Distance to next checkpoint
            // smaller distance → closer → higher rank
            return A.DistanceToNextCheckpoint.CompareTo(B.DistanceToNextCheckpoint);
        });
    }
}
