using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator : MonoBehaviour
{
    [Header("Frames")]
    [SerializeField] private Sprite[] frames;

    [Header("Playback")]
    [SerializeField, Min(1f)] private float framesPerSecond = 8f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnEnable = true;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float frameTimer;
    private bool isPlaying;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyFrame(0);
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length == 0)
        {
            return;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / framesPerSecond;

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1;
                    isPlaying = false;
                }
            }

            ApplyFrame(currentFrame);

            if (!isPlaying)
            {
                break;
            }
        }
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void Restart()
    {
        currentFrame = 0;
        frameTimer = 0f;
        ApplyFrame(currentFrame);
        Play();
    }

    public void SetFrames(Sprite[] newFrames)
    {
        frames = newFrames;
        Restart();
    }

    private void ApplyFrame(int index)
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, frames.Length - 1);
        spriteRenderer.sprite = frames[index];
    }
}
