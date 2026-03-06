using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TopDownCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Rigidbody2D targetRb;

    [Header("Follow")]
    public float followSpeed = 8f;
    public float lookAheadDistance = 2f;

    [Header("Zoom")]
    public float baseOrthoSize = 5f;
    public float zoomAmount = 1.5f;
    public float maxSpeed = 10f;

    [Header("Tilt")]
    public float normalTilt = 3f;     // A / D
    public float driftTilt = 7f;      // Shift + A / D
    public float tiltLerpSpeed = 6f;

    [Header("Shake")]
    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.1f;

    [Header("Bounds")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Camera cam;
    private Vector3 shakeOffset;
    private float shakeTimer;

   void Awake()
{
    cam = GetComponent<Camera>();
    cam.orthographic = true;
    cam.orthographicSize = baseOrthoSize;

    // Lock local rotation at spawn
    transform.localRotation = Quaternion.identity;
}
    void LateUpdate()
    {
        if (target == null || targetRb == null) return;

        Vector2 velocity = targetRb.linearVelocity;
        Vector3 lookAhead = Vector3.zero;

        if (velocity.magnitude > 0.1f)
            lookAhead = (Vector3)(velocity.normalized * lookAheadDistance);

        Vector3 targetPos = target.position + lookAhead;
        targetPos.z = transform.position.z;

        targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);

        Vector3 smoothPos = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );

        UpdateShake();
        transform.position = smoothPos + shakeOffset;

        UpdateZoom(velocity.magnitude);
        UpdateTilt();
    }

    void UpdateZoom(float speed)
    {
        float t = Mathf.Clamp01(speed / maxSpeed);
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            baseOrthoSize + t * zoomAmount,
            Time.deltaTime * 5f
        );
    }

    void UpdateTilt()
    {
        float steerInput = Input.GetAxisRaw("Horizontal");
        bool isDrifting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        float tiltAmount = isDrifting ? driftTilt : normalTilt;
        float tiltZ = -steerInput * tiltAmount;

    }

    void UpdateShake()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            shakeOffset = Random.insideUnitSphere * shakeStrength;
            shakeOffset.z = 0f;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }

    public void TriggerShake()
    {
        shakeTimer = shakeDuration;
    }
}
