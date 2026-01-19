using UnityEngine;

public class LapCounter2D : MonoBehaviour
{
    public int currentLap = 0;
    public int totalLap = 3;
    private bool canCount = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canCount)
        {
            if (currentLap < totalLap)
            {
                currentLap++;
                Debug.Log("Lap: " + currentLap + " / " + totalLap);
            }

            if (currentLap >= totalLap)
            {
                Debug.Log("FINISH!");
                // Dừng xe hoặc hiện màn thắng
            }

            canCount = false;
            Invoke(nameof(ResetCount), 1.5f);
        }
    }

    void ResetCount()
    {
        canCount = true;
    }
}