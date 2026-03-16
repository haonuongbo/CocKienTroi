using UnityEngine;

public class AICarController : MonoBehaviour
{
    [Header("Cài đặt AI - Đường đi")]
    public WaypointCircuit circuit;
    public float waypointThreshold = 3f;

    [Header("Cài đặt Chống Lạc")]
    public float maxTimeToBeLost = 8f; // Tối đa 3 giây không chạm mốc sẽ bị dịch chuyển về

    [Header("Stats (Thông số Vật lý)")]
    public float acceleration = 12f;      
    public float maxSpeed = 10f;
    public float turnSpeed = 150f;
    public float driftTurnMultiplier = 1.5f;
    public float driftFactor = 0.95f;     
    public float driftSlide = 0.4f;       
    public float minTurnSpeed = 0.2f;

    private Rigidbody2D rb;
    
    // Biến nội bộ AI (Lái xe)
    private int currentWaypointIndex = 0;
    private float currentSteerInput = 0f; 
    private bool isDrifting = false;      

    // Biến xử lý kẹt tường & lạc đường
    private float stuckTimer;
    private bool isStuck;
    private float throttleInput = 1f;
    private float lostTimer = 0f; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Tự động tìm đường nếu quên kéo vào Inspector
        if (circuit == null)
            circuit = FindObjectOfType<WaypointCircuit>();
    }

  void Update()
    {
        // An toàn: Nếu không có đường thì đứng im
        if (circuit == null || circuit.waypoints.Count == 0) return;

        // 1. Tìm điểm mốc hiện tại đang nhắm tới
        Transform targetNode = circuit.waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, targetNode.position);

        // Kỹ thuật mới: Tính toán xem Node đang ở TRƯỚC MẶT hay SAU LƯNG xe
        Vector2 directionToTarget = (targetNode.position - transform.position).normalized;
        float dotProduct = Vector2.Dot(-transform.up, directionToTarget); // -up là hướng mũi xe

        // --- KIỂM TRA CHẠM MỐC ---
        // Xe được tính là chạm mốc khi: 
        // 1. Vào đúng vòng tròn (distance < threshold)
        // 2. HOẶC đã chạy ngang qua nó (mốc nằm ở sau lưng: dotProduct < 0) và khoảng cách không quá xa
        if (distance < waypointThreshold || (distance < waypointThreshold * 3f && dotProduct < 0f))
        {
            // Đã chạm hoặc vượt qua mốc -> Chuyển sang mốc tiếp theo
            currentWaypointIndex = (currentWaypointIndex + 1) % circuit.waypoints.Count;
            lostTimer = 0f; // Reset bộ đếm đi lạc
        }
        else
        {
            // --- KIỂM TRA BỊ LẠC ĐƯỜNG ---
            lostTimer += Time.deltaTime; 
            
            if (lostTimer > maxTimeToBeLost)
            {
                RescueLostCar(); // Gọi Lakitu ra cứu!
                return; // Ngừng tính toán frame này để xe được đặt xuống an toàn
            }
        }

        // 2. Tính toán góc lái và Drift
        CalculateAIInput(targetNode.position);

        // 3. Kiểm tra xem xe có đang húc đầu vào tường không
        CheckStuck();
    }

    // ==========================================
    // HÀM CỨU HỘ XE BỊ LẠC (Bản Chuẩn Xác 100%)
    // ==========================================
    void RescueLostCar()
    {
        // Điểm an toàn nhất là điểm ngay trước điểm đang cố đi tới
        int lastPassedIndex = currentWaypointIndex - 1;
        
        if (lastPassedIndex < 0) 
        {
            lastPassedIndex = circuit.waypoints.Count - 1;
        }

        // 1. Đặt xe thẳng vào mốc vừa đi qua
        transform.position = circuit.waypoints[lastPassedIndex].position;

        // 2. Quay đầu xe nhìn thẳng về mốc tiếp theo
        Vector2 directionToNext = (Vector2)circuit.waypoints[currentWaypointIndex].position - (Vector2)transform.position;
        if (directionToNext != Vector2.zero)
        {
            transform.up = -directionToNext.normalized; // Trừ đi vì mũi xe là -up
        }

        // 3. Xóa hết đà bay, đà trượt (Phanh khẩn cấp)
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 4. Reset toàn bộ trạng thái tâm lý của AI
        lostTimer = 0f;
        stuckTimer = 0f;
        isStuck = false;
        throttleInput = 1f;
        currentSteerInput = 0f;
        isDrifting = false;

        Debug.Log("⚠️ " + gameObject.name + " đã được giải cứu về mốc số: " + lastPassedIndex);
    }

    // ==========================================
    // LOGIC LÁI XE CỦA AI
    // ==========================================
    void CalculateAIInput(Vector3 targetPos)
    {
        if (isStuck) return; // Đang kẹt thì để hàm ResetStuck tự lái lùi

        Vector2 vectorToTarget = targetPos - transform.position;
        Vector2 forwardDirection = -transform.up; 

        float angleToTarget = Vector2.SignedAngle(forwardDirection, vectorToTarget);

        // Logic Drift: Vào cua gắt thì drift, cua xong thì nhả ra
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

        // Xoay vô lăng
        float targetSteer = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
        
        if (isDrifting)
        {
            targetSteer = (angleToTarget > 0) ? 1f : -1f; // Ép lái gắt khi Drift
        }

        // Vặn vô lăng từ từ (mượt) chứ không giật cục
        currentSteerInput = Mathf.MoveTowards(currentSteerInput, targetSteer, Time.deltaTime * 5f);
        throttleInput = 1f; // Luôn đạp ga tới
    }

    // ==========================================
    // XỬ LÝ VẬT LÝ (Chạy, Xoay, Trượt)
    // ==========================================
    void FixedUpdate()
    {
        // 1. Lực đẩy ga
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(-transform.up * acceleration * throttleInput);
        }

        // 2. Xoay xe
        if (rb.linearVelocity.magnitude > minTurnSpeed || isStuck)
        {
            float currentTurnSpeed = isDrifting ? turnSpeed * driftTurnMultiplier : turnSpeed;
            float direction = (throttleInput < 0) ? -1 : 1; // Nếu lùi thì bẻ vô lăng ngược lại
            
            rb.MoveRotation(rb.rotation + (currentSteerInput * direction) * currentTurnSpeed * Time.fixedDeltaTime);
        }

        // 3. Lực bám đường (Drift Physics)
        Vector2 velocity = rb.linearVelocity;
        Vector2 forwardDir = -transform.up;
        Vector2 rightDir = transform.right;

        float forwardMag = Vector2.Dot(velocity, forwardDir);
        float sideMag = Vector2.Dot(velocity, rightDir);

        float targetGrip = isDrifting ? driftSlide : driftFactor;
        
        sideMag = Mathf.Lerp(sideMag, sideMag * targetGrip, Time.fixedDeltaTime * 8f);

        rb.linearVelocity = forwardDir * forwardMag + rightDir * sideMag;
    }

    // ==========================================
    // CÁC HÀM HỖ TRỢ
    // ==========================================

    // Gọi khi chạm vào Route Switcher để rẽ sang đường khác
    public void SwitchCircuit(WaypointCircuit newCircuit, int newStartNodeIndex)
    {
        circuit = newCircuit;
        currentWaypointIndex = newStartNodeIndex;
        
        // Reset sạch sẽ tâm lý AI khi sang đường mới
        isStuck = false;
        stuckTimer = 0f;
        isDrifting = false;
        lostTimer = 0f; 
    }

    // Kiểm tra xe có bị kẹt vào chướng ngại vật không
    void CheckStuck()
    {
        // Nếu xe có ga nhưng vận tốc quá chậm (< 0.5) trong 1.5 giây -> Bị kẹt
        if (rb.linearVelocity.magnitude < 0.5f && !isStuck)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1.5f)
            {
                isStuck = true;
                throttleInput = -1f; // Cài số lùi
                currentSteerInput = -currentSteerInput; // Đánh lái ngược lại
                Invoke("ResetStuck", 1.2f); // Lùi trong 1.2 giây rồi đi tiếp
            }
        }
    }

    // Giải trừ trạng thái kẹt
    void ResetStuck()
    {
        isStuck = false;
        stuckTimer = 0f;
        throttleInput = 1f; // Cài lại số tiến
        currentSteerInput = 0f;
    }

    // Vẽ đường màu xanh/đỏ để dễ nhìn trong Scene (Debug)
    void OnDrawGizmos()
    {
        if (circuit != null && circuit.waypoints != null && circuit.waypoints.Count > 0)
        {
            Gizmos.color = isDrifting ? Color.red : Color.green; 
            Gizmos.DrawLine(transform.position, circuit.waypoints[currentWaypointIndex].position);
        }
    }
}