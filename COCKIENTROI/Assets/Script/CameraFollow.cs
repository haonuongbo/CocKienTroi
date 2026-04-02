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

    [Header("Safety")]
    public bool lockToInitialRotation = true;

    private Camera cam;
    private Quaternion baseRotation;
    private Vector3 shakeOffset;
    private float shakeTimer;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(0.1f, baseOrthoSize);

        baseRotation = transform.rotation;

        if (lockToInitialRotation)
        {
            // In 2D setups, parented cameras can inherit accidental rotations.
            transform.rotation = baseRotation;
        }
    }

    void OnValidate()
    {
        followSpeed = Mathf.Max(0f, followSpeed);
        lookAheadDistance = Mathf.Max(0f, lookAheadDistance);
        baseOrthoSize = Mathf.Max(0.1f, baseOrthoSize);
        zoomAmount = Mathf.Max(0f, zoomAmount);
        maxSpeed = Mathf.Max(0.01f, maxSpeed);
        tiltLerpSpeed = Mathf.Max(0f, tiltLerpSpeed);
        shakeDuration = Mathf.Max(0f, shakeDuration);
        shakeStrength = Mathf.Max(0f, shakeStrength);
    }

    void LateUpdate()
    {
        if (cam == null || target == null || targetRb == null)
        {
            return;
        }

        Vector2 velocity = targetRb.linearVelocity;

        if (!IsFinite(velocity))
        {
            velocity = Vector2.zero;
        }

        Vector3 lookAhead = Vector3.zero;

        if (velocity.magnitude > 0.1f)
        {
            lookAhead = (Vector3)(velocity.normalized * lookAheadDistance);
        }

        Vector3 targetPos = target.position + lookAhead;

        if (!IsFinite(targetPos))
        {
            return;
        }

        targetPos.z = transform.position.z;

        float minX = Mathf.Min(minBounds.x, maxBounds.x);
        float maxX = Mathf.Max(minBounds.x, maxBounds.x);
        float minY = Mathf.Min(minBounds.y, maxBounds.y);
        float maxY = Mathf.Max(minBounds.y, maxBounds.y);

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        Vector3 smoothPos = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );

        if (!IsFinite(smoothPos))
        {
            smoothPos = targetPos;
        }

        UpdateShake();
        transform.position = smoothPos + shakeOffset;

        UpdateZoom(velocity.magnitude);
        UpdateTilt();

        // Prevent Unity mouse event raycasts from hitting invalid camera transforms.
        if (!IsFinite(transform.position))
        {
            transform.position = new Vector3(target.position.x, target.position.y, -10f);
        }
    }

    void UpdateZoom(float speed)
    {
        if (!Mathf.Approximately(maxSpeed, 0f))
        {
            speed = Mathf.Clamp(speed, 0f, maxSpeed);
        }

        float t = Mathf.Clamp01(speed / maxSpeed);
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            baseOrthoSize + t * zoomAmount,
            Time.deltaTime * 5f
        );

        if (!float.IsFinite(cam.orthographicSize) || cam.orthographicSize < 0.1f)
        {
            cam.orthographicSize = baseOrthoSize;
        }
    }

    void UpdateTilt()
    {
        float steerInput = Input.GetAxisRaw("Horizontal");
        bool isDrifting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        float tiltAmount = isDrifting ? driftTilt : normalTilt;
        float tiltZ = -steerInput * tiltAmount;

        Quaternion targetRotation = baseRotation * Quaternion.Euler(0f, 0f, tiltZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltLerpSpeed * Time.deltaTime);
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

    private static bool IsFinite(Vector2 value)
    {
        return float.IsFinite(value.x) && float.IsFinite(value.y);
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }
}
