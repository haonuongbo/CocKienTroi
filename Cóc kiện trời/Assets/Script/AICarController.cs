using UnityEngine;

public class AICarController : MonoBehaviour
{
    [Header("Cài đặt AI - Đường đi")]
    public WaypointCircuit circuit;
    public float waypointThreshold = 4f; // Khoảng cách chuyển mục tiêu

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

    // --- CÁC BIẾN MÔ PHỎNG NÚT BẤM ---
    private float inputVertical;   // Thay cho W (1) hoặc S (-1)
    private float inputHorizontal; // Thay cho A (-1) hoặc D (1)
    private bool inputDrift;       // Thay cho Space

    // Biến xử lý khi xe bị kẹt
    private float stuckTimer;
    private bool isStuck;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Tự tìm đường nếu quên kéo vào
        if (circuit == null) circuit = FindFirstObjectByType<WaypointCircuit>();
    }

    void Update()
    {
        if (circuit == null || circuit.waypoints.Count == 0) return;

        // 1. Xác định điểm đến tiếp theo
        Transform targetNode = circuit.waypoints[currentWaypointIndex];
        if (Vector2.Distance(transform.position, targetNode.position) < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % circuit.waypoints.Count;
        }

        // 2. Tính toán Input (AI "bấm" nút gì?)
        CalculateVirtualInput(targetNode.position);

        // 3. Kiểm tra xem xe có đang kẹt không để lùi lại
        CheckStuck();
    }

    void CalculateVirtualInput(Vector3 targetPos)
    {
        if (isStuck) return; // Nếu đang kẹt thì để hàm gỡ kẹt điều khiển

        Vector2 vectorToTarget = targetPos - transform.position;
        Vector2 forwardDirection = -transform.up; // Hướng xe của bạn là trục Y âm

        // Tính góc lệch (-180 đến 180 độ)
        float angleToTarget = Vector2.SignedAngle(forwardDirection, vectorToTarget);

        // --- MÔ PHỎNG NÚT A/D (Lái) ---
        // Chia góc cho 45 để ra giá trị từ -1 đến 1 (mượt hơn bấm phím)
        // Nếu xe rẽ ngược chiều, hãy thêm dấu trừ (-) trước phép tính này
        inputHorizontal = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);

        // --- MÔ PHỎNG NÚT W (Ga) ---
        // Nếu góc cua quá gắt (> 60 độ) thì giảm ga để không văng
        if (Mathf.Abs(angleToTarget) > 60f)
        {
            inputVertical = 0.5f; 
        }
        else
        {
            inputVertical = 1f; // Chạy hết tốc lực
        }

        // --- MÔ PHỎNG NÚT SPACE (Drift) ---
        // Nếu góc cua gắt hơn 30 độ -> AI tự động drift
        inputDrift = Mathf.Abs(angleToTarget) > 30f;
        
        // Mẹo: Khi drift, AI nên bẻ lái gắt hơn
        if (inputDrift && Mathf.Abs(inputHorizontal) < 0.8f)
        {
            inputHorizontal = (angleToTarget > 0) ? 1f : -1f;
        }
    }

    void FixedUpdate()
    {
        // =========================================================
        // PHẦN VẬT LÝ - COPY Y HỆT TỪ CONTROLLER.CS GỐC CỦA BẠN
        // (Chỉ thay thế Input.GetKey bằng các biến input ảo)
        // =========================================================

        // 1. Xử lý di chuyển (Thay cho Input.GetKey(KeyCode.W))
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            if (inputVertical > 0) // Đang ga tới
            {
                rb.AddForce(-transform.up * acceleration * inputVertical);
            }
            else if (inputVertical < 0) // Đang lùi (khi kẹt)
            {
                rb.AddForce(transform.up * acceleration * Mathf.Abs(inputVertical));
            }
        }

        // 2. Xử lý xoay xe
        if (rb.linearVelocity.magnitude > minTurnSpeed || isStuck)
        {
            float currentTurnSpeed = inputDrift ? turnSpeed * driftTurnMultiplier : turnSpeed;
            
            // inputHorizontal đóng vai trò là chiều xoay (như bấm A/D)
            rb.MoveRotation(rb.rotation + inputHorizontal * currentTurnSpeed * Time.fixedDeltaTime);
        }

        // 3. Xử lý Drift và độ bám đường (Thay cho Input.GetKey(KeyCode.Space))
        Vector2 velocity = rb.linearVelocity;
        Vector2 forwardDir = -transform.up;
        Vector2 rightDir = transform.right;

        float forwardMag = Vector2.Dot(velocity, forwardDir);
        float sideMag = Vector2.Dot(velocity, rightDir);

        float targetGrip = inputDrift ? driftSlide : driftFactor;
        
        // Lerp để trượt mượt mà
        sideMag = Mathf.Lerp(sideMag, sideMag * targetGrip, Time.fixedDeltaTime * 5f);

        // Cập nhật lại vận tốc
        rb.linearVelocity = forwardDir * forwardMag + rightDir * sideMag;
    }

    // Logic gỡ kẹt: Nếu xe đứng yên quá 1 giây thì tự lùi
    void CheckStuck()
    {
        if (rb.linearVelocity.magnitude < 0.2f)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }

        if (stuckTimer > 1f)
        {
            isStuck = true;
            inputVertical = -1f; // Lùi lại
            inputHorizontal = -inputHorizontal; // Bẻ lái ngược lại
            Invoke("ResetStuck", 1.5f); // Lùi trong 1.5 giây
        }
    }

    void ResetStuck()
    {
        isStuck = false;
        stuckTimer = 0f;
    }

    void OnDrawGizmos()
    {
        if (circuit != null && circuit.waypoints.Count > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, circuit.waypoints[currentWaypointIndex].position);
        }
    }
}