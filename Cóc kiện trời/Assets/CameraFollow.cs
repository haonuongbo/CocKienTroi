using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 baseOffset = new Vector3(0f, 0f, -7.5f);
    [Header("Drift")]
    public float minDriftSpeed = 3f;

    [Header("Follow")]
    public float followSmooth = 5f;

    [Header("Look Ahead")]
    public float lookAheadDistance = 2f;
    public float lookAheadSmooth = 3f;

    private Vector3 currentLookAhead;

    void LateUpdate()
    {
        if (target == null) return;

        // direction the car is facing (forward)
        Vector3 forward = -target.up;

        // desired look-ahead offset
        Vector3 targetLookAhead = forward * lookAheadDistance;

        // smooth look-ahead change (slow response on turns)
        currentLookAhead = Vector3.Lerp(
            currentLookAhead,
            targetLookAhead,
            Time.deltaTime * lookAheadSmooth
        );

        // final target position
        Vector3 desiredPos = target.position + baseOffset + currentLookAhead;

        // smooth camera movement
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            Time.deltaTime * followSmooth
        );
    }
}
