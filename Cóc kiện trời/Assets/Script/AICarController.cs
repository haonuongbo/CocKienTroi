using UnityEngine;
<<<<<<< HEAD

public class AICarController : MonoBehaviour
=======
using Unity.Netcode;


public class AICarController : NetworkBehaviour 
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
{
    [Header("Cài đặt AI - Đường đi")]
    public WaypointCircuit circuit;
    public float waypointThreshold = 3f;

    [Header("Stats (Giống Controller.cs)")]
<<<<<<< HEAD
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
=======
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

>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
    private float stuckTimer;
    private bool isStuck;
    private float throttleInput = 1f;

<<<<<<< HEAD
=======
    public string circuitName = "Map_3Circuit";

>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
<<<<<<< HEAD

        if (circuit == null)
            circuit = FindObjectOfType<WaypointCircuit>();
=======
    }

    public override void OnNetworkSpawn()
    {
         Debug.Log($"AICar OnNetworkSpawn IsServer:{IsServer} IsOwner:{IsOwner} IsHost:{IsHost} IsClient:{IsClient}");
    Debug.Log($"circuit:{(circuit!=null)} animator:{(animator!=null)} rb:{(rb!=null)}");
        base.OnNetworkSpawn();

        if (circuit == null)
        {   
            GameObject circuitObj = null;
            // Try tag-based lookup first (safer if you tag your Main circuit as 'MainCircuit')
            try
            {
                circuitObj = GameObject.FindWithTag("MainCircuit");
            }
            catch (UnityException)
            {
                // Tag may not exist in project; ignore and fall back
            }

            if (circuitObj != null)
            {
                WaypointCircuit found = circuitObj.GetComponent<WaypointCircuit>();
                if (found != null) circuit = found;
            }

            // Fallback to named object (if provided) or any circuit in scene
            if (circuit == null && !string.IsNullOrEmpty(circuitName))
            {
                GameObject go = GameObject.Find(circuitName);
                if (go != null) circuit = go.GetComponent<WaypointCircuit>();
            }

            if (circuit == null)
                circuit = FindObjectOfType<WaypointCircuit>();
        }

        // Auto-find animator if not set on the inspector
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (animator == null)
                animator = FindFirstObjectByType<Animator>(); // last resort: any animator in scene
        }
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
    }

    void Update()
    {
        if (circuit == null || circuit.waypoints.Count == 0) return;

<<<<<<< HEAD
        // 1. Tìm điểm đến
=======
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

>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
        Transform targetNode = circuit.waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, targetNode.position);

        if (distance < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % circuit.waypoints.Count;
        }

<<<<<<< HEAD
        // 2. Tính toán Logic lái (Bộ não)
        CalculateAIInput(targetNode.position);

        // 3. Kiểm tra kẹt
=======
        CalculateAIInput(targetNode.position);
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
        CheckStuck();
    }

    void CalculateAIInput(Vector3 targetPos)
    {
        if (isStuck) return;

        Vector2 vectorToTarget = targetPos - transform.position;
<<<<<<< HEAD
        Vector2 forwardDirection = -transform.up; // Hướng mũi xe

        float angleToTarget = Vector2.SignedAngle(forwardDirection, vectorToTarget);

        // --- CẢI TIẾN 1: LOGIC DRIFT "DẺO" (HYSTERESIS) ---
        // Nếu chưa Drift: Cần góc lớn (> 35 độ) mới bắt đầu Drift
        // Nếu ĐANG Drift: Cần góc rất nhỏ (< 15 độ) mới chịu dừng Drift
        // -> Giúp xe giữ trạng thái Drift lâu hơn, không bị bật/tắt liên tục
=======
        Vector2 forwardDirection = -transform.up;

        float angleToTarget = Vector2.SignedAngle(forwardDirection, vectorToTarget);

>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
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

<<<<<<< HEAD
        // --- CẢI TIẾN 2: LÀM MƯỢT TAY LÁI (SMOOTH STEERING) ---
        float targetSteer = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);
        
        // Khi Drift thì bẻ lái gắt hơn
=======
        float targetSteer = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);

>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
        if (isDrifting)
        {
            targetSteer = (angleToTarget > 0) ? 1f : -1f;
        }

<<<<<<< HEAD
        // Thay vì gán trực tiếp, ta dùng MoveTowards để xoay vô lăng từ từ
        // Tốc độ trả lái là 5f (có thể chỉnh tăng giảm độ nhạy)
        currentSteerInput = Mathf.MoveTowards(currentSteerInput, targetSteer, Time.deltaTime * 5f);
        
=======
        currentSteerInput = Mathf.MoveTowards(currentSteerInput, targetSteer, Time.deltaTime * 5f);
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
        throttleInput = 1f;
    }

    void FixedUpdate()
    {
<<<<<<< HEAD
        // --- XỬ LÝ VẬT LÝ ---

        // 1. Lực đẩy
=======
      if (!IsOwner) return;
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(-transform.up * acceleration * throttleInput);
        }

<<<<<<< HEAD
        // 2. Xoay xe (Dùng currentSteerInput đã được làm mượt)
        if (rb.linearVelocity.magnitude > minTurnSpeed || isStuck)
        {
            float currentTurnSpeed = isDrifting ? turnSpeed * driftTurnMultiplier : turnSpeed;
            
            // Xử lý khi lùi thì lái ngược
=======
        if (rb.linearVelocity.magnitude > minTurnSpeed || isStuck)
        {
            float currentTurnSpeed = isDrifting ? turnSpeed * driftTurnMultiplier : turnSpeed;
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
            float direction = (throttleInput < 0) ? -1 : 1;

            rb.MoveRotation(rb.rotation + (currentSteerInput * direction) * currentTurnSpeed * Time.fixedDeltaTime);
        }

<<<<<<< HEAD
        // 3. Xử lý Trượt (Drift Physics)
=======
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
        Vector2 velocity = rb.linearVelocity;
        Vector2 forwardDir = -transform.up;
        Vector2 rightDir = transform.right;

        float forwardMag = Vector2.Dot(velocity, forwardDir);
        float sideMag = Vector2.Dot(velocity, rightDir);

<<<<<<< HEAD
        // Độ bám đường thay đổi mượt mà
        float targetGrip = isDrifting ? driftSlide : driftFactor;
        
        // Lerp mượt hơn chút (tăng từ 5f lên 8f để bám lại đường nhanh hơn sau khi drift)
=======
        float targetGrip = isDrifting ? driftSlide : driftFactor;
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
        sideMag = Mathf.Lerp(sideMag, sideMag * targetGrip, Time.fixedDeltaTime * 8f);

        rb.linearVelocity = forwardDir * forwardMag + rightDir * sideMag;
    }

<<<<<<< HEAD
    // --- CÁC HÀM PHỤ (Gỡ kẹt & Vẽ đường) ---
=======
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
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
<<<<<<< HEAD
                currentSteerInput = -currentSteerInput; // Đảo lái
                Invoke("ResetStuck", 1.2f);
=======
                currentSteerInput = -currentSteerInput;
                Invoke(nameof(ResetStuck), 1.2f);
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
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
<<<<<<< HEAD
            Gizmos.color = isDrifting ? Color.red : Color.green; // Đổi màu khi Drift để dễ debug
            Gizmos.DrawLine(transform.position, circuit.waypoints[currentWaypointIndex].position);
        }
    }
}
=======
            Gizmos.color = isDrifting ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, circuit.waypoints[currentWaypointIndex].position);
        }
    }
}
>>>>>>> ed402a53569365e48b82cdf6c92882c9500609e7
