using UnityEngine;

public class ControlSpeedAnim : MonoBehaviour
{
    [Header("Stats")]
    public float acceleration = 10.1f;
    public float maxSpeed = 8f;
    public float turnSpeed = 120f;
    public float driftTurnMultiplier = 1.5f;
    public float driftFactor = 0.9f;
    public float driftSlide = 0.6f;
    public float minTurnSpeed = 0.2f;

    [Header("Drift")]
    public float minDriftSpeed = 3f;

    [Header("Animation")]
    public Animator animator;
    public float minAnimSpeed = 0.5f;
    public float maxAnimSpeed = 2.0f;

    private Rigidbody2D rb;

    // mobile input states
    private float steerInput; // -1 = right, 1 = left
    private float keyboardSteer;  // from A/D keys
    private bool drifting;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        // keyboard steering (A = left, D = right)
        // Shift + A/D = drift
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKey(KeyCode.A))
            keyboardSteer = 1f;
        else if (Input.GetKey(KeyCode.D))
            keyboardSteer = -1f;
        else
            keyboardSteer = 0f;

        // keyboard drift: engage when Shift + A or D, disengage otherwise
        if (shiftHeld && keyboardSteer != 0f && rb.linearVelocity.magnitude >= minDriftSpeed)
            drifting = true;
        else if (shiftHeld && keyboardSteer == 0f)
            drifting = false;
        else if (!shiftHeld && drifting && keyboardSteer != 0f)
            drifting = false;

        // animation control
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
        // accelerate
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(-transform.up * acceleration);
        }

        // steering — keyboard takes priority over UI buttons
        float effectiveSteer = keyboardSteer != 0f ? keyboardSteer : steerInput;
        if (rb.linearVelocity.magnitude > minTurnSpeed)
        {
            float currentTurnSpeed = drifting
                ? turnSpeed * driftTurnMultiplier
                : turnSpeed;

            rb.MoveRotation(
                rb.rotation + effectiveSteer * currentTurnSpeed * Time.fixedDeltaTime
            );
        }

        // drift physics
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

    public void TurnLeftDown()
    {
        steerInput = 1f;
    }

    public void TurnRightDown()
    {
        steerInput = -1f;
    }

    public void TurnRelease()
    {
        steerInput = 0f;
    }

    public void DriftDown()
    {
        if (rb.linearVelocity.magnitude >= minDriftSpeed)
            drifting = true;
    }

    public void DriftRelease()
    {
        drifting = false;
    }
}
