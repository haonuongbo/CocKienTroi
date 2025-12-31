using UnityEngine;

public class AnimationSpeedByVelocity : MonoBehaviour
{
    public Rigidbody2D carRb;
    public Animator animator;

    [Header("Speed Mapping")]
  public float maxCarSpeed = 20f;
    public float minAnimSpeed = 0.5f;
    public float maxAnimSpeed = 2.0f;

    void Update()
    {
        if (carRb == null || animator == null)
            return;

        float speed = carRb.linearVelocity.magnitude;

        // normalize speed (0 → 1)
        float normalizedSpeed = Mathf.Clamp01(speed / maxCarSpeed);

        // map to animation speed range
        animator.speed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, normalizedSpeed);
    }
}
