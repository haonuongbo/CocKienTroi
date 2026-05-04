using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class IntroVideoToScene : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "CharacterSelection";

    private VideoPlayer videoPlayer;
    private bool hasLoadedNextScene;

    private float sceneLoadTime;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.isLooping = false;
        sceneLoadTime = Time.unscaledTime;
    }

    private void OnEnable()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += HandleVideoFinished;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= HandleVideoFinished;
        }
    }

    private void Update()
    {
        // Chờ 0.5s sau khi load scene để tránh nhận diện nhầm thao tác chạm còn sót lại từ scene trước (Splash Art)
        if (Time.unscaledTime - sceneLoadTime < 0.5f) return;

        // Chỉ nhận diện các thao tác chạm MỚI (Began) hoặc nhấn nút MỚI (Down)
        bool isTouchBegan = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool isMouseDown = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
        bool isKeyDown = Input.anyKeyDown;

        if (isTouchBegan || isMouseDown || isKeyDown)
        {
            SkipVideo();
        }
    }

    private void SkipVideo()
    {
        if (hasLoadedNextScene || string.IsNullOrWhiteSpace(nextSceneName))
        {
            return;
        }

        hasLoadedNextScene = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    private void HandleVideoFinished(VideoPlayer source)
    {
        SkipVideo();
    }
}