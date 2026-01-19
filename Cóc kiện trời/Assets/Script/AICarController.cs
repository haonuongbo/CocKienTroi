using UnityEngine;

public class AICarController : MonoBehaviour
{
    [Header("Cài đặt AI - Đường đi")]
    public WaypointCircuit circuit;
    public float waypointThreshold = 3f;

    [Header("Stats (Giống Controller.cs)")]
    public float acceleration = 12f;      // Tăng tốc độ lên chút cho AI bám đuổi tốt hơn
    public float maxSpeed = 10f;
    public float turnSpeed = 150f;
    public float driftTurnMultiplier = 1.5f;
    public float driftFactor = 0.95f;     // Bám đường tốt khi chạy thẳng
    public float driftSlide = 0.4f;       // Trượt nhiều hơn khi drift (số càng nhỏ càng trượt)
    public float minTurnSpeed = 0.2f;

    private Rigidbody2D rb;
    
    // Biến nội bộ AI
    private int currentWaypointIndex = 0;
    private float currentSteerInput = 0f; // Dùng để làm mượt tay lái
    private bool isDrifting = false;      // Trạng thái Drift hiện tại

    // Xử lý kẹt
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

        // 1. Tìm điểm đến
        Transform targetNode = circuit.waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, targetNode.position);

        if (distance < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % circuit.waypoints.Count;
        }

        // 2. Tính toán Logic lái (Bộ não)
        CalculateAIInput(targetNode.position);

        // 3. Kiểm tra kẹt
        CheckStuck();
    }

    void CalculateAIInput(Vector3 targetPos)
    {
        if (isStuck) return;

        Vector2 vectorToTarget = targetPos - transform.position;
        Vector2 forwardDirection = -transform.up; // Hướng mũi xe

        float angleToTarget = Vector2.SignedAngle(forwardDirection, vectorToTarget);

        // --- CẢI TIẾN 1: LOGIC DRIFT "DẺO" (HYSTERESIS) ---
        // Nếu chưa Drift: Cần góc lớn (> 35 độ) mới bắt đầu Drift
        // Nếu ĐANG Drift: Cần góc rất nhỏ (< 15 độ) mới chịu dừng Drift
        // -> Giúp xe giữ trạng thái Drift lâu hơn, không bị bật/tắt liên tục
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

        // --- CẢI TIẾN 2: LÀM MƯỢT TAY LÁI (SMOOTH STEERING) ---
        float targetSteer = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
        
        // Khi Drift thì bẻ lái gắt hơn
        if (isDrifting)
        {
            targetSteer = (angleToTarget > 0) ? 1f : -1f;
        }

        // Thay vì gán trực tiếp, ta dùng MoveTowards để xoay vô lăng từ từ
        // Tốc độ trả lái là 5f (có thể chỉnh tăng giảm độ nhạy)
        currentSteerInput = Mathf.MoveTowards(currentSteerInput, targetSteer, Time.deltaTime * 5f);
        
        throttleInput = 1f;
    }

    void FixedUpdate()
    {
        // --- XỬ LÝ VẬT LÝ ---

        // 1. Lực đẩy
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(-transform.up * acceleration * throttleInput);
        }

        // 2. Xoay xe (Dùng currentSteerInput đã được làm mượt)
        if (rb.linearVelocity.magnitude > minTurnSpeed || isStuck)
        {
            float currentTurnSpeed = isDrifting ? turnSpeed * driftTurnMultiplier : turnSpeed;
            
            // Xử lý khi lùi thì lái ngược
            float direction = (throttleInput < 0) ? -1 : 1;

            rb.MoveRotation(rb.rotation + (currentSteerInput * direction) * currentTurnSpeed * Time.fixedDeltaTime);
        }

        // 3. Xử lý Trượt (Drift Physics)
        Vector2 velocity = rb.linearVelocity;
        Vector2 forwardDir = -transform.up;
        Vector2 rightDir = transform.right;

        float forwardMag = Vector2.Dot(velocity, forwardDir);
        float sideMag = Vector2.Dot(velocity, rightDir);

        // Độ bám đường thay đổi mượt mà
        float targetGrip = isDrifting ? driftSlide : driftFactor;
        
        // Lerp mượt hơn chút (tăng từ 5f lên 8f để bám lại đường nhanh hơn sau khi drift)
        sideMag = Mathf.Lerp(sideMag, sideMag * targetGrip, Time.fixedDeltaTime * 8f);

        rb.linearVelocity = forwardDir * forwardMag + rightDir * sideMag;
    }

    // --- CÁC HÀM PHỤ (Gỡ kẹt & Vẽ đường) ---
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
                currentSteerInput = -currentSteerInput; // Đảo lái
                Invoke("ResetStuck", 1.2f);
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
            Gizmos.color = isDrifting ? Color.red : Color.green; // Đổi màu khi Drift để dễ debug
            Gizmos.DrawLine(transform.position, circuit.waypoints[currentWaypointIndex].position);
        }
    }
}