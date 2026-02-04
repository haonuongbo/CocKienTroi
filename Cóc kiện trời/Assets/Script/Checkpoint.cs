using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Checkpoint order index (0 → N-1)")]
    public int checkpointIndex;

    private void OnTriggerEnter2D(Collider2D other)
    {
        RaceProgressTracker tracker = other.GetComponent<RaceProgressTracker>();
        if (tracker == null) return;

        tracker.OnCheckpointHit(checkpointIndex);
    }
}
