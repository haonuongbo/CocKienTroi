using UnityEngine;

public class AICarController : MonoBehaviour
{
    [Header("Cài đặt AI - Đường đi")]
    public WaypointCircuit circuit;
    public float waypointThreshold = 3f;

    [Header("Stats (Giống Controller.cs)")]
    public float acceleration = 12f;
    public float maxSpeed = 10f;
    public float turnSpeed = 150f;
    public float driftTurnMultiplier = 1.5f;
    public float driftFactor = 0.95f;
    public float driftSlide = 0.4f;
    public float minTurnSpeed = 0.2f;

    [Header("Animation")]
    public Animator animator;
    public float minAnimSpeed = 0.5f;
    public float maxAnimSpeed = 2.0f;

    private Rigidbody2D rb;

    private int currentWaypointIndex = 0;
    private float currentSteerInput = 0f;
    private bool isDrifting = false;

    private float stuckTimer;
    private bool isStuck;
    private float throttleInput = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        if (circuit == null)
            circuit = FindObjectOfType<WaypointCircuit>();
    }

    void Update()
    {
        if (circuit == null || circuit.waypoints.Count == 0) return;

        // ===== ANIMATION CONTROL (GIỐNG SCRIPT TRÊN) =====
        if (animator != null)
        {
            if (isDrifting)
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
        // ===============================================

        Transform targetNode = circuit.waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, targetNode.position);

        if (distance < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % circuit.waypoints.Count;
        }

        CalculateAIInput(targetNode.position);
        CheckStuck();
    }

    void CalculateAIInput(Vector3 targetPos)
    {
        if (isStuck) return;

        Vector2 vectorToTarget = targetPos - transform.position;
        Vector2 forwardDirection = -transform.up;

        float angleToTarget = Vector2.SignedAngle(forwardDirection, vectorToTarget);

        float enterDriftAngle = 35f;
        float exitDriftAngle = 15f;

        if (!isDrifting)
        {
            if (Mathf.Abs(angleToTarget) > enterDriftAngle) isDrifting = true;
        }
        else
        {
            if (Mathf.Abs(angleToTarget) < exitDriftAngle) isDrifting = false;
        }

        float targetSteer = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);

        if (isDrifting)
        {
            targetSteer = (angleToTarget > 0) ? 1f : -1f;
        }

        currentSteerInput = Mathf.MoveTowards(currentSteerInput, targetSteer, Time.deltaTime * 5f);
        throttleInput = 1f;
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(-transform.up * acceleration * throttleInput);
        }

        if (rb.linearVelocity.magnitude > minTurnSpeed || isStuck)
        {
            float currentTurnSpeed = isDrifting ? turnSpeed * driftTurnMultiplier : turnSpeed;
            float direction = (throttleInput < 0) ? -1 : 1;

            rb.MoveRotation(rb.rotation + (currentSteerInput * direction) * currentTurnSpeed * Time.fixedDeltaTime);
        }

        Vector2 velocity = rb.linearVelocity;
        Vector2 forwardDir = -transform.up;
        Vector2 rightDir = transform.right;

        float forwardMag = Vector2.Dot(velocity, forwardDir);
        float sideMag = Vector2.Dot(velocity, rightDir);

        float targetGrip = isDrifting ? driftSlide : driftFactor;
        sideMag = Mathf.Lerp(sideMag, sideMag * targetGrip, Time.fixedDeltaTime * 8f);

        rb.linearVelocity = forwardDir * forwardMag + rightDir * sideMag;
    }

    public void SwitchCircuit(WaypointCircuit newCircuit, int newStartNodeIndex)
    {
        circuit = newCircuit;
        currentWaypointIndex = newStartNodeIndex;
        isStuck = false;
        stuckTimer = 0;
        isDrifting = false;
    }

    void CheckStuck()
    {
        if (rb.linearVelocity.magnitude < 0.5f && !isStuck)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1.5f)
            {
                isStuck = true;
                throttleInput = -1f;
                currentSteerInput = -currentSteerInput;
                Invoke(nameof(ResetStuck), 1.2f);
            }
        }
    }

    void ResetStuck()
    {
        isStuck = false;
        stuckTimer = 0f;
        throttleInput = 1f;
        currentSteerInput = 0f;
    }

    void OnDrawGizmos()
    {
        if (circuit != null && circuit.waypoints.Count > 0)
        {
            Gizmos.color = isDrifting ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, circuit.waypoints[currentWaypointIndex].position);
        }
    }
}
