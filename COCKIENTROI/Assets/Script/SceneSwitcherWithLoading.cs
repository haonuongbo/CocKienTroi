using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcherWithLoading : MonoBehaviour
{
    public string loadingSceneName = "Loading";

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetString("NextSceneToLoad", sceneName);
        SceneManager.LoadScene(loadingSceneName);
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("NextSceneToLoad", currentSceneName);
        SceneManager.LoadScene(loadingSceneName);
    }
}
