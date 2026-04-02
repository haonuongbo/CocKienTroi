using UnityEngine;

public class LapTriggerChild : MonoBehaviour
{
    private LapManager2D lapManager;

    private int lastTrackerLap = 0;

    void Awake()
    {
        lapManager = GetComponentInParent<LapManager2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name + " đã chạm vào vạch đích!");
            // Chỉ bắt vòng của Người Chơi
            if (other.GetComponent<ControlSpeedAnim>() != null)
            {
                RaceProgressTracker tracker = other.GetComponent<RaceProgressTracker>();
                
                // Cơ chế bảo mật: Chỉ đếm vòng LÊN UI khi xe đã thực sự ăn đủ số Checkpoint trong 1 vòng đua
                // Nếu xe vừa xuất phát dẫm trúng vạch, tracker.CurrentLap vẫn là 0, nó sẽ bỏ qua!
                if (tracker != null && tracker.CurrentLap > lastTrackerLap)
                {
                    lastTrackerLap = tracker.CurrentLap;
                    lapManager.CountLap();
                }
            }
        }
    }
}
