using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Phiên bản joystick ảo của ControlSpeedAnimMobile.
/// Thay thế hai nút Trái/Phải bằng một joystick hình tròn gồm:
///   - joystickBackground : hình nền (viền ngoài)
///   - joystickThumb      : nút kéo bên trong
/// Kéo ngón tay sang trái/phải để lái. Nút Drift giữ nguyên như cũ.
/// </summary>
public class ControlSpeedAnimMobileJoystick : MonoBehaviour
{
    [Header("Stats")]
    public float acceleration         = 3f;
    public float maxSpeed             = 7f;
    public float turnSpeed            = 70f;
    public float driftTurnMultiplier  = 1.8f;
    public float driftFactor          = 0.6f;
    public float driftSlide           = 0.7f;
    public float minTurnSpeed         = 0f;

    [Header("Joystick UI")]
    [Tooltip("Kéo RectTransform của hình nền joystick vào đây")]
    public RectTransform joystickBackground;
    [Tooltip("Kéo RectTransform của nút di chuyển (thumb) vào đây")]
    public RectTransform joystickThumb;
    [Tooltip("Tên object nền joystick để tự tìm nếu chưa gán tay")]
    public string joystickBackgroundName = "JoystickBackground";
    [Tooltip("Tên object nút joystick (thumb) để tự tìm nếu chưa gán tay")]
    public string joystickThumbName = "JoystickThumb";
    [Tooltip("Vùng chết ở giữa (0-1). Nhỏ hơn giá trị này thì không lái)")]
    [Range(0f, 0.4f)]
    public float deadZone = 0.1f;

    [Header("Drift Button")]
    [Tooltip("Tên GameObject của nút Drift trong Scene")]
    public string btnDriftName = "BtnDrift";

    [Header("Auto Bind")]
    [Tooltip("Số lần thử tự tìm UI sau khi spawn")]
    public int autoBindRetryCount = 20;
    [Tooltip("Khoảng chờ giữa mỗi lần thử tìm UI")]
    public float autoBindRetryInterval = 0.1f;

    [Header("Drift")]
    public float minDriftSpeed = 3f;

    [Header("Animation")]
    public Animator animator;
    public float minAnimSpeed = 0.5f;
    public float maxAnimSpeed = 1.25f;

    // ── runtime ──────────────────────────────────────────────
    private Rigidbody2D rb;
    private Vector2 joystickInput;
    private bool    joystickDragging;
    private bool    driftHeld;
    private bool    drifting;

    private float   joystickRadius;
    private Vector2 thumbDefaultPos;
    private Canvas  parentCanvas;
    private bool    joystickBound;
    private bool    driftBound;

    // ─────────────────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        StartCoroutine(InitializeBindingsAfterSpawn());
    }

    IEnumerator InitializeBindingsAfterSpawn()
    {
        int tries = Mathf.Max(1, autoBindRetryCount);
        float wait = Mathf.Max(0.01f, autoBindRetryInterval);

        for (int i = 0; i < tries; i++)
        {
            AutoFindUIObjects();

            if (!joystickBound)
                SetupJoystick();

            if (!driftBound)
                BindDriftButton();

            if (joystickBound && driftBound)
                yield break;

            yield return new WaitForSeconds(wait);
        }

        if (!joystickBound)
            Debug.LogWarning("[ControlSpeedAnimMobileJoystick] Không bind được joystick. Kiểm tra tên object hoặc gán tay trong Inspector.");
        if (!driftBound)
            Debug.LogWarning($"[ControlSpeedAnimMobileJoystick] Không bind được nút drift: {btnDriftName}");
    }

    void AutoFindUIObjects()
    {
        if (joystickBackground == null && !string.IsNullOrEmpty(joystickBackgroundName))
        {
            GameObject bgObj = GameObject.Find(joystickBackgroundName);
            if (bgObj != null)
                joystickBackground = bgObj.GetComponent<RectTransform>();
        }

        if (joystickThumb == null && !string.IsNullOrEmpty(joystickThumbName))
        {
            GameObject thumbObj = GameObject.Find(joystickThumbName);
            if (thumbObj != null)
                joystickThumb = thumbObj.GetComponent<RectTransform>();
        }
    }

    // ── Khởi tạo joystick ────────────────────────────────────
    void SetupJoystick()
    {
        if (joystickBound) return;

        if (joystickBackground == null)
        {
            return;
        }

        if (joystickThumb == null)
        {
            return;
        }

        joystickRadius  = joystickBackground.rect.width * 0.5f;
        thumbDefaultPos = joystickThumb != null ? joystickThumb.anchoredPosition : Vector2.zero;
        parentCanvas    = joystickBackground.GetComponentInParent<Canvas>();

        EnsureRaycastTarget(joystickBackground);
        EnsureRaycastTarget(joystickThumb);

        // Bind ở cả nền và thumb để kéo được dù bắt đầu bấm vào object nào.
        BindJoystickPointerEvents(joystickBackground);
        BindJoystickPointerEvents(joystickThumb);

        joystickBound = true;
    }

    void BindJoystickPointerEvents(RectTransform target)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>()
                            ?? target.gameObject.AddComponent<EventTrigger>();

        AddEntry(trigger, EventTriggerType.PointerDown, OnJoystickDown);
        AddEntry(trigger, EventTriggerType.Drag,        OnJoystickDrag);
        AddEntry(trigger, EventTriggerType.EndDrag,     OnJoystickUp);
        AddEntry(trigger, EventTriggerType.PointerUp,   OnJoystickUp);
    }

    static void EnsureRaycastTarget(RectTransform target)
    {
        Image img = target.GetComponent<Image>();
        if (img != null)
            img.raycastTarget = true;
    }

    static void AddEntry(EventTrigger et, EventTriggerType type,
                         UnityEngine.Events.UnityAction<BaseEventData> cb)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(cb);
        et.triggers.Add(entry);
    }

    // ── Xử lý sự kiện joystick ───────────────────────────────
    void OnJoystickDown(BaseEventData data)
    {
        joystickDragging = true;
        MoveThumb(((PointerEventData)data).position);
    }

    void OnJoystickDrag(BaseEventData data)
    {
        if (!joystickDragging) return;
        MoveThumb(((PointerEventData)data).position);
    }

    void OnJoystickUp(BaseEventData data)
    {
        joystickDragging = false;
        joystickInput = Vector2.zero;
        if (joystickThumb != null)
            joystickThumb.anchoredPosition = thumbDefaultPos;
    }

    void MoveThumb(Vector2 screenPos)
    {
        if (joystickBackground == null || joystickThumb == null) return;

        // Lấy camera đúng theo chế độ Canvas
        Camera cam = (parentCanvas != null &&
                      parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? parentCanvas.worldCamera
            : null;

        // Chuyển toạ độ màn hình → toạ độ local của joystick
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground, screenPos, cam, out Vector2 localPos);

        // Giới hạn thumb trong hình tròn nền
        Vector2 clamped = Vector2.ClampMagnitude(localPos, joystickRadius);
        joystickThumb.anchoredPosition = clamped;

        // Chuẩn hoá joystick theo cả 2 trục để quay theo hướng kéo (trái/phải/lên/xuống)
        Vector2 rawInput = clamped / joystickRadius;

        // Dead zone theo độ lớn vector
        if (rawInput.magnitude < deadZone)
            joystickInput = Vector2.zero;
        else
            joystickInput = Vector2.ClampMagnitude(rawInput, 1f);
    }

    // ── Nút Drift ────────────────────────────────────────────
    void BindDriftButton()
    {
        if (driftBound) return;

        GameObject btn = GameObject.Find(btnDriftName);
        if (btn == null)
        {
            return;
        }

        EventTrigger trigger = btn.GetComponent<EventTrigger>()
                            ?? btn.AddComponent<EventTrigger>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => driftHeld = true);
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => driftHeld = false);
        trigger.triggers.Add(up);

        driftBound = true;
    }

    // ── Logic giống hệt bản gốc ───────────────────────────────
    void Update()
    {
        drifting = driftHeld && joystickInput.sqrMagnitude > 0.0001f && rb.linearVelocity.magnitude >= minDriftSpeed;

        if (animator != null)
        {
            animator.speed = drifting
                ? 0f
                : Mathf.Lerp(minAnimSpeed, maxAnimSpeed,
                      Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed));
        }
    }

    void FixedUpdate()
    {
        // Rotation is controlled manually via joystick; cancel physics spin from collisions.
        if (joystickInput.sqrMagnitude <= 0.0001f)
            rb.angularVelocity = 0f;

        // Tăng tốc
        if (rb.linearVelocity.magnitude < maxSpeed)
            rb.AddForce(-transform.up * acceleration);

        // Lái
        if (rb.linearVelocity.magnitude > minTurnSpeed && joystickInput.sqrMagnitude > 0.0001f)
        {
            rb.angularVelocity = 0f;
            float ts = drifting ? turnSpeed * driftTurnMultiplier : turnSpeed;

            // Map hướng joystick sang hướng mặt xe:
            // kéo xuống => xe quay xuống, kéo lên => xe quay lên, tương tự trái/phải.
            Vector2 desiredDirection = joystickInput.normalized;
            float targetAngle = Vector2.SignedAngle(Vector2.down, desiredDirection);
            float nextAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, ts * Time.fixedDeltaTime);

            rb.MoveRotation(nextAngle);
        }

        // Drift physics
        Vector2 velocity   = rb.linearVelocity;
        Vector2 forwardDir = -transform.up;
        Vector2 rightDir   = transform.right;

        float forwardMag = Vector2.Dot(velocity, forwardDir);
        float sideMag    = Vector2.Dot(velocity, rightDir);

        float grip = drifting ? driftSlide : driftFactor;
        sideMag = Mathf.Lerp(sideMag, sideMag * grip, Time.fixedDeltaTime * 5f);

        rb.linearVelocity = forwardDir * forwardMag + rightDir * sideMag;
    }
}
