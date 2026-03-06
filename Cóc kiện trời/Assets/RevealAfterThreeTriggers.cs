using UnityEngine;
using UnityEngine.SceneManagement;

public class RevealAfterThreeTriggers : MonoBehaviour
{
    [Header("UI/scene hooks")]
    public MonoBehaviour[] scriptsToDisable;  // Scripts that update it
    public RaceRankUI raceRankUI;        // assign your RaceRankUI instance
    public string winSceneName = "WinScene";  // change to your actual scene name

    private int triggerCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        triggerCount++;

        if (triggerCount >= 1)
        {
            // send the final ranking information to the carrier singleton
            if (raceRankUI != null)
                raceRankUI.SendRankingsToCarrier();

            // optional: stop other scripts if you still want
            foreach (var script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = false;
            }

            // switch to win scene
            if (!string.IsNullOrEmpty(winSceneName))
                SceneManager.LoadScene(winSceneName);
        }
    }
}