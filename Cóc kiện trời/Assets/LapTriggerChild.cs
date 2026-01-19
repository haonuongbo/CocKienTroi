using UnityEngine;

public class LapTriggerChild : MonoBehaviour
{
    private LapManager2D lapManager;

    void Awake()
    {
        lapManager = GetComponentInParent<LapManager2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            lapManager.CountLap();
        }
    }
}
