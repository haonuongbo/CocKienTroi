using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CharacterSelectionBackgroundMedia : MonoBehaviour
{
    [Header("Background Video")]
    [SerializeField] private VideoClip backgroundVideo;
    [SerializeField] private bool loopVideo = true;

    [Header("Background Music (separate from video)")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField] private bool loopMusic = true;

    private VideoPlayer videoPlayer;
    private AudioSource musicSource;

    private void Start()
    {
        Image staticBackground = GetComponent<Image>();
        if (staticBackground != null)
        {
            staticBackground.enabled = false;
        }

        SetupVideoOnMainCamera();
        SetupBackgroundMusic();
    }

    private void SetupVideoOnMainCamera()
    {
        if (backgroundVideo == null || Camera.main == null)
        {
            return;
        }

        GameObject cameraObject = Camera.main.gameObject;
        videoPlayer = cameraObject.GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = cameraObject.AddComponent<VideoPlayer>();
        }

        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.clip = backgroundVideo;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.Play();
    }

    private void SetupBackgroundMusic()
    {
        if (backgroundMusic == null)
        {
            return;
        }

        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = true;
        musicSource.loop = loopMusic;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
        musicSource.clip = backgroundMusic;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
}
