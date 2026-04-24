using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class IntroVideoToScene : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "CharacterSelection";

    private VideoPlayer videoPlayer;
    private bool hasLoadedNextScene;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.isLooping = false;
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
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.touchCount > 0)
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