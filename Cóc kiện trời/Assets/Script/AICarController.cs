using UnityEngine;

public class AICarController : MonoBehaviour
{
    [Header("Cài đặt AI - Đường đi")]
    public WaypointCircuit circuit;
    public float waypointThreshold = 3f; 

    [Header("Thông số Vật Lý (Phải giống hệt Controller.cs)")]
    public float acceleration = 10f;
    public float maxSpeed = 8f;
    public float turnSpeed = 120f;
    public float driftTurnMultiplier = 1.5f;
    public float driftFactor = 0.9f;
    public float driftSlide = 0.5f;
    public float minTurnSpeed = 0.2f;

    private Rigidbody2D rb;
    private int currentWaypointIndex = 0;

    private float inputVertical;   
    private float inputHorizontal; 
    private bool inputDrift;       

    private float stuckTimer;
    private bool isStuck;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Đảm bảo Rigidbody không bị rơi
        rb.gravityScale = 0f;

        // Tìm Circuit nếu chưa kéo vào Inspector
        if (circuit == null) 
        {
            circuit = GameObject.FindObjectOfType<WaypointCircuit>();
        }
    }

    void Update()
    {
        // Kiểm tra an toàn: Nếu không có đường đi hoặc danh sách điểm trống thì thoát ngay
        if (circuit == null || circuit.waypoints == null || circuit.waypoints.Count == 0) return;

        // 1. Lấy điểm mục tiêu
        Transform targetNode = circuit.waypoints[currentWaypointIndex];
        
        // Kiểm tra nếu Node bị xóa hoặc mất
        if (targetNode == null) return;

        // 2. Kiểm tra khoảng cách để chuyển sang điểm tiếp theo
        float distanceToTarget = Vector2.Distance(transform.position, targetNode.position);
        if (distanceToTarget < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % circuit.waypoints.Count;
        }

        // 3. Tính toán điều khiển
        CalculateVirtualInput(targetNode.position);

        // 4. Kiểm tra kẹt
        CheckStuck();
    }

    public void SwitchCircuit(WaypointCircuit newCircuit, int newStartNodeIndex)
    {
        if (newCircuit == null) return;
        circuit = newCircuit;
        currentWaypointIndex = Mathf.Clamp(newStartNodeIndex, 0, newCircuit.waypoints.Count - 1);
        stuckTimer = 0f; 
        isStuck = false;
    }

    void CalculateVirtualInput(Vector3 targetPos)
    {
        if (isStuck) return;

        Vector2 vectorToTarget = targetPos - transform.position;
        // HƯỚNG XE: Theo code gốc của bạn là -transform.up
        Vector2 forwardDirection = -transform.up; 

        // Tính góc giữa hướng xe và hướng đến đích
        float angleToTarget = Vector2.SignedAngle(forwardDirection, vectorToTarget);

        // --- SỬA LỖI LÁI ---
        // Nếu xe rẽ ngược hướng, hãy bỏ dấu trừ (-) ở dòng dưới
        inputHorizontal = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);

        // --- GA (Luôn tiến tới) ---
        inputVertical = 1f;

        // --- DRIFT (Nếu cua gắt > 30 độ) ---
        inputDrift = Mathf.Abs(angleToTarget) > 30f;
        
        if (inputDrift)
        {
            // Ép lái mạnh hơn khi drift
            inputHorizontal = (angleToTarget > 0) ? 1f : -1f;
        }
    }

    void FixedUpdate()
    {
        // 1. Xử lý lực đẩy (Thêm Force)
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            // Lực luôn tác động theo hướng tới của xe (-transform.up)
            Vector2 thrustDir = (inputVertical >= 0) ? -transform.up : transform.up;
            rb.AddForce(thrustDir * acceleration * Mathf.Abs(inputVertical));
        }

        // 2. Xử lý xoay xe (Rotation)
        if (rb.linearVelocity.magnitude > minTurnSpeed || isStuck)
        {
            float currentTurnSpeed = inputDrift ? turnSpeed * driftTurnMultiplier : turnSpeed;
            // Áp dụng xoay
            rb.MoveRotation(rb.rotation + inputHorizontal * currentTurnSpeed * Time.fixedDeltaTime);
        }

        // 3. Xử lý trượt ngang (Ma sát bên)
        Vector2 velocity = rb.linearVelocity;
        Vector2 forwardDir = -transform.up;
        Vector2 rightDir = transform.right;

        float forwardMag = Vector2.Dot(velocity, forwardDir);
        float sideMag = Vector2.Dot(velocity, rightDir);

        float targetGrip = inputDrift ? driftSlide : driftFactor;
        sideMag = Mathf.Lerp(sideMag, sideMag * targetGrip, Time.fixedDeltaTime * 5f);

        rb.linearVelocity = forwardDir * forwardMag + rightDir * sideMag;
    }

    void CheckStuck()
    {
        // Nếu xe gần như đứng yên trong khi đang đạp ga
        if (rb.linearVelocity.magnitude < 0.3f && !isStuck)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1.2f)
            {
                isStuck = true;
                inputVertical = -1f; // Lùi
                inputHorizontal = -inputHorizontal; // Lái ngược
                Invoke("ResetStuck", 1.0f); 
            }
        }
    }

    void ResetStuck()
    {
        isStuck = false;
        stuckTimer = 0f;
        inputVertical = 1f;
    }

    void OnDrawGizmos()
    {
        if (circuit != null && circuit.waypoints != null && circuit.waypoints.Count > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, circuit.waypoints[currentWaypointIndex].position);
        }
    }
}