using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [Header("---- UI LOADING ----")]
    public Image progressBarFill;       // Progress bar fill image (dùng PNG)
    public Image backgroundImage;      // Background image

    [Header("---- CÀI ĐẶT ----")]
    [SerializeField] private string sceneToLoad = "GameScene";  // Đổi tên scene ở đây
    public float minimumLoadTime = 3.5f;       // Thời gian loading tối thiểu (tăng để thấy progress)

    private AsyncOperation asyncLoad;
    private float loadStartTime;

    void Start()
    {
        loadStartTime = Time.time;
        
        // Kiểm tra nếu có scene được pass từ MapSelectionUI
        if (PlayerPrefs.HasKey("NextSceneToLoad"))
        {
            sceneToLoad = PlayerPrefs.GetString("NextSceneToLoad");
            PlayerPrefs.DeleteKey("NextSceneToLoad"); // Xóa sau khi đã lấy
            Debug.Log($"Loading scene từ PlayerPrefs: {sceneToLoad}");
        }
        
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // ===== BẮT ĐẦU LOAD SCENE =====
        asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        float loadProgress = 0f;
        float startTime = Time.time;

        while (asyncLoad.progress < 0.9f || Time.time - startTime < minimumLoadTime)
        {
            float elapsedTime = Time.time - startTime;
            float targetProgress = asyncLoad.progress;

            // Progress dựa trên thời gian (để chắc chắn hiển thị mượt mà)
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            float asyncProgress = asyncLoad.progress;
            
            // Lấy cái nào lớn hơn
            targetProgress = Mathf.Max(asyncProgress, timeProgress * 0.9f);

            // Smooth lerp - giảm tốc độ để thấy rõ hơn
            loadProgress = Mathf.Lerp(loadProgress, targetProgress, Time.deltaTime * 2f);

            UpdateProgressBar(loadProgress);

            yield return null;
        }

        // ===== HOÀN THÀNH =====
        UpdateProgressBar(1f);
        Debug.Log("Loading Complete!");
        
        yield return new WaitForSeconds(0.3f);

        // ===== ACTIVATE SCENE =====
        asyncLoad.allowSceneActivation = true;
        
        yield return new WaitForSeconds(0.5f);
    }

    void UpdateProgressBar(float progress)
    {
        if (progressBarFill != null)
        {
            // Cập nhật fillAmount cho progress bar
            progressBarFill.fillAmount = progress;
        }
    }
}
