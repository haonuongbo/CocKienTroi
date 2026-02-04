using System.Collections;
using UnityEngine;
using Unity.Netcode;
[RequireComponent(typeof(Camera))]
public class TopDownCameraFollow : NetworkBehaviour
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
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Try to pick a sensible target on spawn (local player if available)
        SetupTarget();

        if (NetworkManager.Singleton != null)
        {
            // Listen for client connects so we can re-try finding a local player if needed
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        // If this client connected and we don't yet have a target, try finding our player.
        if (NetworkManager.Singleton == null) return;
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            StartCoroutine(DelayedSetup());
        }
    }

    IEnumerator DelayedSetup()
    {
        // Wait a frame to let player spawn complete
        yield return null;
        SetupTarget();
    }

    void SetupTarget()
    {
        if (target != null && targetRb != null) return; // already set

        // Try to find the local player's PCController by matching OwnerClientId
        if (NetworkManager.Singleton != null)
        {
            ulong localId = NetworkManager.Singleton.LocalClientId;
            var players = FindObjectsOfType<PCController>();
            foreach (var p in players)
            {
                if (p.NetworkObject != null && p.NetworkObject.OwnerClientId == localId)
                {
                    SetTargetToPlayer(p.transform, p.GetComponent<Rigidbody2D>());
                    return;
                }
            }

            // If we're the server/host and didn't find a local-owned player, try following the host player's object
            if (NetworkManager.Singleton.IsServer)
            {
                foreach (var p in players)
                {
                    if (p.NetworkObject != null && p.NetworkObject.OwnerClientId == NetworkManager.ServerClientId)
                    {
                        SetTargetToPlayer(p.transform, p.GetComponent<Rigidbody2D>());
                        return;
                    }
                }
            }
        }

        // Fallback: try any PCController in scene
        var anyPlayer = FindObjectOfType<PCController>();
        if (anyPlayer != null)
        {
            SetTargetToPlayer(anyPlayer.transform, anyPlayer.GetComponent<Rigidbody2D>());
        }
    }

    void SetTargetToPlayer(Transform t, Rigidbody2D rb2d)
    {
        target = t;
        targetRb = rb2d;
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

        Quaternion targetRot = Quaternion.Euler(0f, 0f, tiltZ);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * tiltLerpSpeed
        );
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
