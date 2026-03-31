using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Checkpoint order index (0 → N-1)")]
    public int checkpointIndex;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Dùng GetComponentInParent phòng trường hợp Collider của xe AI nằm ở object con
        RaceProgressTracker tracker = other.GetComponentInParent<RaceProgressTracker>();
        if (tracker == null) return;

        tracker.OnCheckpointHit(checkpointIndex);
    }
}
