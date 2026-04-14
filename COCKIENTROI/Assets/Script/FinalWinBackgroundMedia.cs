using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Plays background video and optional background music for FINAL WIN scene.
/// It hides the static layout background image and renders video on Main Camera.
/// </summary>
public class FinalWinBackgroundMedia : MonoBehaviour
{
    [Header("Media")]
    [SerializeField] private VideoClip backgroundVideo;
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Playback")]
    [SerializeField] private bool loopVideo = true;
    [SerializeField] private bool loopMusic = true;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f;

    [Header("Layout")]
    [SerializeField] private string backgroundImagePath = "LayoutRoot/Background";

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;

    private IEnumerator Start()
    {
        // Wait one frame so layout builder can create UI hierarchy before we hide static bg.
        yield return null;

        DisableStaticBackgroundImage();

        Camera targetCamera = EnsureMainCamera();
        if (targetCamera == null)
        {
            Debug.LogError("FinalWinBackgroundMedia: Could not create or find Main Camera.");
            yield break;
        }

        SetupBackgroundVideo(targetCamera);
        SetupBackgroundMusic(targetCamera);
    }

    private void DisableStaticBackgroundImage()
    {
        if (string.IsNullOrWhiteSpace(backgroundImagePath))
        {
            return;
        }

        Transform bg = transform.Find(backgroundImagePath);
        if (bg == null)
        {
            return;
        }

        Image backgroundImage = bg.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.enabled = false;
        }
    }

    private Camera EnsureMainCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            return mainCamera;
        }

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";

        Camera cameraComponent = cameraObject.AddComponent<Camera>();
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = Color.black;
        cameraComponent.nearClipPlane = 0.3f;
        cameraComponent.farClipPlane = 1000f;

        if (cameraObject.GetComponent<AudioListener>() == null)
        {
            cameraObject.AddComponent<AudioListener>();
        }

        return cameraComponent;
    }

    private void SetupBackgroundVideo(Camera targetCamera)
    {
        if (backgroundVideo == null)
        {
            Debug.LogWarning("FinalWinBackgroundMedia: No background video assigned.");
            return;
        }

        videoPlayer = targetCamera.GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = targetCamera.gameObject.AddComponent<VideoPlayer>();
        }

        videoPlayer.clip = backgroundVideo;
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = targetCamera;
        videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.Play();
    }

    private void SetupBackgroundMusic(Camera targetCamera)
    {
        if (backgroundMusic == null)
        {
            return;
        }

        audioSource = targetCamera.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = targetCamera.gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = backgroundMusic;
        audioSource.volume = musicVolume;
        audioSource.loop = loopMusic;
        audioSource.playOnAwake = true;
        audioSource.Play();
    }
}
