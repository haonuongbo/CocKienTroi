using UnityEngine;

public class RevealAndLockCanvas : MonoBehaviour
{
    public GameObject canvasObject;          // Canvas GameObject
    public MonoBehaviour[] scriptsToDisable; // Scripts that update the canvas
    private int triggerCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        triggerCount++;

        if (triggerCount >= 3)
        {
            canvasObject.SetActive(true);

            foreach (MonoBehaviour script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = false;
            }
        }
    }
}