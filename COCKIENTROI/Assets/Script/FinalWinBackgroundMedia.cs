using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Plays background video/music and fills FINAL WIN summary UI from championship data.
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

    [Header("Summary Targets (Optional)")]
    [SerializeField] private Image playerCharacterImage;
    [SerializeField] private Image titleImage;
    [SerializeField] private Image starsImage;
    [SerializeField] private Image rankBadgeImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rankText;

    [Header("Character Setup By Index")]
    [SerializeField] private Sprite[] characterSpritesByIndex;
    [SerializeField] private Sprite[] titleSpritesByIndex;

    [Header("Result Setup")]
    [Tooltip("Index 0 = 1 star, Index 4 = 5 stars")]
    [SerializeField] private Sprite[] starsSpritesByCount;
    [Tooltip("Index 0 = Rank 1, Index 4 = Rank 5")]
    [SerializeField] private Sprite[] rankBadgeSpritesByRank;

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;

    private IEnumerator Start()
    {
        // Wait one frame so layout builder can create UI hierarchy before we hide static bg.
        yield return null;

        DisableStaticBackgroundImage();
        ResolveSummaryTargets();

        Camera targetCamera = EnsureMainCamera();
        if (targetCamera == null)
        {
            Debug.LogError("FinalWinBackgroundMedia: Could not create or find Main Camera.");
            yield break;
        }

        SetupBackgroundVideo(targetCamera);
        SetupBackgroundMusic(targetCamera);
        ApplyChampionshipSummary();
    }

    private void DisableStaticBackgroundImage()
    {
        if (string.IsNullOrWhiteSpace(backgroundImagePath))
            return;

        Transform bg = transform.Find(backgroundImagePath);
        if (bg == null)
            return;

        Image backgroundImage = bg.GetComponent<Image>();
        if (backgroundImage != null)
            backgroundImage.enabled = false;
    }

    private void ResolveSummaryTargets()
    {
        if (playerCharacterImage == null)
            playerCharacterImage = FindImageByName("PlayerCharacter");

        if (titleImage == null)
            titleImage = FindImageByName("Title");

        if (starsImage == null)
            starsImage = FindImageByName("Stars");

        if (rankBadgeImage == null)
            rankBadgeImage = FindImageByName("RankBadge");

        if (titleText == null)
            titleText = FindTextByName("TitleText");

        if (rankText == null)
            rankText = FindTextByName("RankText");
    }

    private Image FindImageByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return null;

        Image[] allImages = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < allImages.Length; i++)
        {
            if (allImages[i] != null && allImages[i].name == objectName)
                return allImages[i];
        }

        return null;
    }

    private TMP_Text FindTextByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return null;

        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < allTexts.Length; i++)
        {
            if (allTexts[i] != null && allTexts[i].name == objectName)
                return allTexts[i];
        }

        return null;
    }

    private void ApplyChampionshipSummary()
    {
        int selectedCharacterIndex = ChampionshipProgress.GetSelectedCharacterIndex();
        string selectedCharacterName = ChampionshipProgress.GetSelectedCharacterName();

        if (playerCharacterImage != null && selectedCharacterIndex >= 0 && selectedCharacterIndex < characterSpritesByIndex.Length)
        {
            Sprite selectedSprite = characterSpritesByIndex[selectedCharacterIndex];
            if (selectedSprite != null)
                playerCharacterImage.sprite = selectedSprite;
        }

        if (titleImage != null && selectedCharacterIndex >= 0 && selectedCharacterIndex < titleSpritesByIndex.Length)
        {
            Sprite selectedTitleSprite = titleSpritesByIndex[selectedCharacterIndex];
            if (selectedTitleSprite != null)
                titleImage.sprite = selectedTitleSprite;
        }

        if (titleText != null)
            titleText.text = selectedCharacterName;

        int roundedRank = ChampionshipProgress.GetRoundedAverageRank();
        int stars = ChampionshipProgress.GetStarsFromRoundedRank(roundedRank);

        if (starsImage != null && stars >= 1 && stars <= starsSpritesByCount.Length)
        {
            Sprite starsSprite = starsSpritesByCount[stars - 1];
            if (starsSprite != null)
                starsImage.sprite = starsSprite;
        }

        if (rankBadgeImage != null && roundedRank >= 1 && roundedRank <= rankBadgeSpritesByRank.Length)
        {
            Sprite rankBadgeSprite = rankBadgeSpritesByRank[roundedRank - 1];
            if (rankBadgeSprite != null)
                rankBadgeImage.sprite = rankBadgeSprite;
        }

        if (rankText != null)
        {
            string m1 = ChampionshipProgress.TryGetMapRank("M1", out int rankM1) ? rankM1.ToString() : "-";
            string m2 = ChampionshipProgress.TryGetMapRank("M2", out int rankM2) ? rankM2.ToString() : "-";
            string m3 = ChampionshipProgress.TryGetMapRank("M3", out int rankM3) ? rankM3.ToString() : "-";

            rankText.text = $"M1:{m1}  M2:{m2}  M3:{m3}  AVG RANK:{roundedRank}  STARS:{stars}";
        }
    }

    private Camera EnsureMainCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
            return mainCamera;

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";

        Camera cameraComponent = cameraObject.AddComponent<Camera>();
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = Color.black;
        cameraComponent.nearClipPlane = 0.3f;
        cameraComponent.farClipPlane = 1000f;

        if (cameraObject.GetComponent<AudioListener>() == null)
            cameraObject.AddComponent<AudioListener>();

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
            videoPlayer = targetCamera.gameObject.AddComponent<VideoPlayer>();

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
            return;

        audioSource = targetCamera.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = targetCamera.gameObject.AddComponent<AudioSource>();

        audioSource.clip = backgroundMusic;
        audioSource.volume = musicVolume;
        audioSource.loop = loopMusic;
        audioSource.playOnAwake = true;
        audioSource.Play();
    }
}
