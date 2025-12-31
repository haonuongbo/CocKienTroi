using UnityEngine;

public class ControlSpeedAnim : MonoBehaviour
{
    [Header("Stats")]
    public float acceleration = 10f;
    public float maxSpeed = 8f;
    public float turnSpeed = 120f;
    public float driftTurnMultiplier = 1.5f;
    public float driftFactor = 0.9f;
    public float driftSlide = 0.6f;
    public float minTurnSpeed = 0.2f;

    [Header("Animation")]
    public Animator animator;
    public float minAnimSpeed = 0.5f;
    public float maxAnimSpeed = 2.0f;

    private Rigidbody2D rb;
    private bool drifting;
    private float steerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        // drift state
        drifting = Input.GetKey(KeyCode.LeftShift);

        // steering input
        steerInput = 0f;
        if (Input.GetKey(KeyCode.A)) steerInput = 1f;
        if (Input.GetKey(KeyCode.D)) steerInput = -1f;

        // animation control
        if (animator != null)
        {
            if (drifting)
            {
                // drifting → animation stopped
                animator.speed = 0f;
            }
            else
            {
                // normal driving → animation speed based on velocity
                float speed = rb.linearVelocity.magnitude;
                float normalizedSpeed = Mathf.Clamp01(speed / maxSpeed);
                animator.speed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, normalizedSpeed);
            }
        }
    }

    void FixedUpdate()
    {
        // accelerate to fixed max speed
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
}

