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

    // Called by checkpoint trigger
    public void OnCheckpointHit(int checkpointIndex)
    {
        int expectedIndex = checkpointsPassed % checkpoints.Length;

        if (checkpointIndex != expectedIndex)
            return;

        checkpointsPassed++;

        if (checkpointIndex == checkpoints.Length - 1)
        {
            currentLap++;
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
