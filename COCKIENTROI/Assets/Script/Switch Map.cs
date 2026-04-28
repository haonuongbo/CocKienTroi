using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchMap : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetString("NextSceneToLoad", sceneName);
        SceneManager.LoadScene("Loading");
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("NextSceneToLoad", currentScene);
        SceneManager.LoadScene("Loading");
    }
}
