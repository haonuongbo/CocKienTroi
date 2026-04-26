using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Manages background video playback for the MAP ICON scene.
/// Disables the static sprite image and plays video on the Main Camera with separate audio.
/// </summary>
public class MapIconBackgroundMedia : MonoBehaviour
{
    [SerializeField] private VideoClip backgroundVideo;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool loopVideo = true;
    [SerializeField] private bool loopMusic = true;
    [SerializeField] private float musicVolume = 0.7f;

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;

    private void Start()
    {
        // Disable the static background image
        Image backgroundImage = GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.enabled = false;
        }

        // Setup video on Main Camera
        SetupBackgroundVideo();

        // Setup audio
        SetupBackgroundMusic();
    }

    private void SetupBackgroundVideo()
    {
        if (backgroundVideo == null)
        {
            Debug.LogWarning("MapIconBackgroundMedia: No background video clip assigned!");
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("MapIconBackgroundMedia: Main Camera not found!");
            return;
        }

        // Check if VideoPlayer already exists
        videoPlayer = mainCamera.GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = mainCamera.gameObject.AddComponent<VideoPlayer>();
        }

        videoPlayer.clip = backgroundVideo;
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = mainCamera;
        videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // Disable video audio for separate track
        videoPlayer.Play();
    }

    private void SetupBackgroundMusic()
    {
        if (backgroundMusic == null)
        {
            Debug.LogWarning("MapIconBackgroundMedia: No background music clip assigned!");
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("MapIconBackgroundMedia: Main Camera not found!");
            return;
        }

        // Check if AudioSource already exists for background music
        audioSource = mainCamera.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = mainCamera.gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = backgroundMusic;
        audioSource.volume = musicVolume;
        audioSource.loop = loopMusic;
        audioSource.playOnAwake = true;
        audioSource.Play();
    }
}
