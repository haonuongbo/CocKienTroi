using UnityEngine;

public class RaceProgressTracker : MonoBehaviour
{
    // Expose runtime-readonly views backed by the fields below
    public int CurrentLap => currentLap;
    public int CheckpointsPassed => checkpointsPassed;
    public float DistanceToNextCheckpoint => distanceToNext;

    [Header("Track Settings")]
    public Transform[] checkpoints;     // Index 0 → N-1
    public int totalLaps;

    [Header("Progress (Read Only)")]
    // Use fields (not properties) so Unity Inspector can show them
    public int currentLap;
    public int checkpointsPassed;

    [SerializeField] // keep it private but visible in inspector if you want to debug it
    private float distanceToNext;

    void Awake()
    {
        currentLap = 0;
        checkpointsPassed = 0;
    }

    public void OnCheckpointHit(int checkpointIndex)
    {
        int expectedIndex = checkpointsPassed % checkpoints.Length;
        int diff = checkpointIndex - expectedIndex;

        // Nếu hiệu số âm, tức là xe nhảy từ cuối vòng (vd 23) qua đầu vòng mới (vd 0)
        if (diff < 0)
        {
            diff += checkpoints.Length;
        }

        // Cho phép xe nhảy cóc tối đa quá ngã rẽ 1 nửa vòng đua (Nửa map). 
        // Lớn hơn thì bị coi là xe đi chui ngược vòng (Hack điểm).
        if (diff >= 0 && diff <= (checkpoints.Length / 2))
        {
            // Cộng bù luôn số điểm bị thiếu nếu lỡ rẽ nhánh bỏ qua vài Node!
            checkpointsPassed += (diff + 1);

            // Bắt vòng tự động từ tổng điểm, đếm chóp luôn cả nút đích bị nhảy cóc
            currentLap = checkpointsPassed / checkpoints.Length;
        }
    }

    void Update()
    {
        int nextCheckpointIndex = checkpointsPassed % checkpoints.Length;
        Transform nextCheckpoint = checkpoints[nextCheckpointIndex];

        distanceToNext = Vector2.Distance(
            transform.position,
            nextCheckpoint.position
        );
    }

    // Ranking value
    public float GetProgressValue()
    {
        /*
         * Higher value = better rank
         * Priority:
         * 1) Lap count
         * 2) Checkpoints passed
         * 3) Distance counting DOWN to next checkpoint (closer = higher)
         */
        return (currentLap * checkpoints.Length + checkpointsPassed) * 10000f
               - distanceToNext;
    }
}
