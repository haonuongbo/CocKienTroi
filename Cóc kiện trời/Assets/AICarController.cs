using UnityEngine;

public class AICarController : MonoBehaviour
{
    [Header("Cài đặt AI")]
    public WaypointCircuit circuit; // Kéo object đường đi vào đây
    public float waypointThreshold = 3f; // Khoảng cách coi như đã "chạm" mốc

    [Header("Thông số xe (Giống người chơi)")]
    public float acceleration = 10f;
    public float maxSpeed = 8f;
    public float turnSpeed = 120f;
    public float driftTurnMultiplier = 1.5f;
    public float driftFactor = 0.9f;
    public float driftSlide = 0.5f;
    public float minTurnSpeed = 0.2f;

    private Rigidbody2D rb;
    private int currentWaypointIndex = 0;
    private float steerInput;
    private bool drifting;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        // Nếu chưa gán đường đi hoặc đường đi không có điểm nào thì không làm gì cả
        if (circuit == null || circuit.waypoints.Count == 0) return;

        // 1. Xác định điểm mục tiêu hiện tại
        Transform targetNode = circuit.waypoints[currentWaypointIndex];

        // 2. Kiểm tra khoảng cách: Nếu xe đến gần điểm này rồi thì chuyển sang điểm kế tiếp
        float distance = Vector2.Distance(transform.position, targetNode.position);
        if (distance < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % circuit.waypoints.Count;
        }

        // 3. Tính toán lái xe
        DriveAI(targetNode.position);
    }

    void DriveAI(Vector3 targetPosition)
    {
        // Tính vector hướng từ xe đến mục tiêu
        Vector2 vectorToTarget = targetPosition - transform.position;
        
        // Hướng mặt của xe (Dựa trên code gốc của bạn là -transform.up)
        Vector2 currentDirection = -transform.up;

        // Tính góc lệch giữa hướng xe và hướng mục tiêu
        float angleToTarget = Vector2.SignedAngle(currentDirection, vectorToTarget);

        // Điều chỉnh tay lái (steerInput)
        // angleToTarget > 0: Mục tiêu ở bên trái -> Rẽ trái (Input = 1)
        // angleToTarget < 0: Mục tiêu ở bên phải -> Rẽ phải (Input = -1)
        // (Lưu ý: Logic trái/phải có thể ngược lại tùy vào hệ trục, ta sẽ điều chỉnh nếu xe chạy ngược)
        
        if (angleToTarget > 5f) 
        {
            steerInput = 1f; // Rẽ trái
        }
        else if (angleToTarget < -5f)
        {
            steerInput = -1f;  // Rẽ phải
        }
        else
        {
            steerInput = 0f;  // Đi thẳng
        }

        // Logic Drift đơn giản: Nếu góc cua gắt (> 45 độ) thì bật drift
        drifting = Mathf.Abs(angleToTarget) > 45f;
    }

    void FixedUpdate()
    {
        // --- Phần vật lý giữ nguyên từ Controller.cs của bạn ---

        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(-transform.up * acceleration);
        }

        if (rb.linearVelocity.magnitude > minTurnSpeed)
        {
            float currentTurnSpeed = drifting
                ? turnSpeed * driftTurnMultiplier
                : turnSpeed;

            rb.MoveRotation(rb.rotation + steerInput * currentTurnSpeed * Time.fixedDeltaTime);
        }

        Vector2 velocity = rb.linearVelocity;
        Vector2 forwardDir = -transform.up;
        Vector2 rightDir = transform.right;

        float forwardMag = Vector2.Dot(velocity, forwardDir);
        float sideMag = Vector2.Dot(velocity, rightDir);

        float targetGrip = drifting ? driftSlide : driftFactor;
        sideMag = Mathf.Lerp(sideMag, sideMag * targetGrip, Time.fixedDeltaTime * 5f);

        rb.linearVelocity = forwardDir * forwardMag + rightDir * sideMag;
    }
}