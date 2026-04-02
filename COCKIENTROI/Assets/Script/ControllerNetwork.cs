using Unity.Netcode;
using UnityEngine;

public class ControllerNetwork : NetworkBehaviour
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
    private bool drifting;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        if(!IsOwner) return;
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

        // steering
        if (rb.linearVelocity.magnitude > minTurnSpeed)
        {
            float currentTurnSpeed = drifting
                ? turnSpeed * driftTurnMultiplier
                : turnSpeed;

            rb.MoveRotation(
                rb.rotation + steerInput * currentTurnSpeed * Time.fixedDeltaTime
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
