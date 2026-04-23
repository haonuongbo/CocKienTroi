using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Mobile-only version of ControlSpeedAnim.
/// Steering and drift are driven entirely by UI buttons.
/// Drift activates when the Drift button is held AND a Turn button is also held.
/// </summary>
public class ControlSpeedAnimMobile : MonoBehaviour, ICarController
{
    [Header("Stats")]
    public float acceleration = 3f;
    public float maxSpeed = 7f;
    public float turnSpeed = 70f;
    public float driftTurnMultiplier = 1.8f;
    public float driftFactor = 0.6f;
    public float driftSlide = 0.7f;
    public float minTurnSpeed = 0f;

    // ===== ICarController =====
    public float MaxSpeed { get => maxSpeed; set => maxSpeed = value; }
    public float DriftSlide { get => driftSlide; set => driftSlide = value; }
    public void SetControlEnabled(bool enabled) { this.enabled = enabled; }

    [Header("Mobile Button Names")]
    public string btnLeftName  = "BtnLeft";
    public string btnRightName = "BtnRight";
    public string btnDriftName = "BtnDrift";

    [Header("Drift")]
    public float minDriftSpeed = 3f;

    [Header("Animation")]
    public Animator animator;
    public float minAnimSpeed = 0.5f;
    public float maxAnimSpeed = 1.25f;

    private Rigidbody2D rb;

    // Button states
    private float steerInput;   // -1 = right, 1 = left, 0 = none
    private bool driftHeld;     // drift button is being held
    private bool drifting;      // physics drift is active

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        ConnectMobileButtons();
    }

    void ConnectMobileButtons()
    {
        BindButton(btnLeftName,  () => TurnLeftDown(),  () => TurnRelease());
        BindButton(btnRightName, () => TurnRightDown(), () => TurnRelease());
        BindButton(btnDriftName, () => DriftDown(),     () => DriftRelease());
    }

    void BindButton(string buttonName, UnityAction onDown, UnityAction onUp)
    {
        GameObject btn = GameObject.Find(buttonName);
        if (btn == null)
        {
            Debug.LogWarning($"[ControlSpeedAnimMobile] Không tìm thấy nút: {buttonName}");
            return;
        }

        EventTrigger trigger = btn.GetComponent<EventTrigger>()
                            ?? btn.AddComponent<EventTrigger>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => onDown());
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => onUp());
        trigger.triggers.Add(up);
    }

    void Update()
    {
        // Drift is active when: drift button held + turning + fast enough
        if (driftHeld && steerInput != 0f && rb.linearVelocity.magnitude >= minDriftSpeed)
            drifting = true;
        else
            drifting = false;

        // Animation
        if (animator != null)
        {
            if (drifting)
            {
                animator.speed = 0f;
            }
            else
            {
                float speed = rb.linearVelocity.magnitude;
                float normalizedSpeed = Mathf.Clamp01(speed / maxSpeed);
                animator.speed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, normalizedSpeed);
            }
        }
    }

    void FixedUpdate()
    {
        // Accelerate
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(-transform.up * acceleration);
        }

        // Steering
        if (rb.linearVelocity.magnitude > minTurnSpeed)
        {
            float currentTurnSpeed = drifting
                ? turnSpeed * driftTurnMultiplier
                : turnSpeed;

            rb.MoveRotation(
                rb.rotation + steerInput * currentTurnSpeed * Time.fixedDeltaTime
            );
        }

        // Drift physics
        Vector2 velocity = rb.linearVelocity;

        Vector2 forwardDir = -transform.up;
        Vector2 rightDir = transform.right;

        float forwardMag = Vector2.Dot(velocity, forwardDir);
        float sideMag = Vector2.Dot(velocity, rightDir);

        float grip = drifting ? driftSlide : driftFactor;
        sideMag = Mathf.Lerp(sideMag, sideMag * grip, Time.fixedDeltaTime * 5f);

        rb.linearVelocity = forwardDir * forwardMag + rightDir * sideMag;
    }

    // ===== UI BUTTON METHODS =====
    // Wire these to EventTrigger (PointerDown / PointerUp) on each button.

    public void TurnLeftDown()  { steerInput = 1f; }
    public void TurnRightDown() { steerInput = -1f; }
    public void TurnRelease()   { steerInput = 0f; }

    public void DriftDown()     { driftHeld = true; }
    public void DriftRelease()  { driftHeld = false; }
}
