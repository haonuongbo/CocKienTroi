using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CarDrift : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnAngle = 120f;
    public float driftFactor = 0.9f;
    public float driftSlide = 0.5f;

    private Rigidbody2D rb;
    private bool driftingLeft = false;
    private bool driftingRight = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        rb.AddForce(-transform.up * moveSpeed);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            driftingLeft = Input.GetKey(KeyCode.A);
            driftingRight = Input.GetKey(KeyCode.D);
        }
        else
        {
            driftingLeft = false;
            driftingRight = false;
        }

        if (rb.linearVelocity.magnitude > 0.2f)
        {
            if (Input.GetKey(KeyCode.A))
            {
                rb.MoveRotation(rb.rotation + turnAngle * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D))
            {
                rb.MoveRotation(rb.rotation - turnAngle * Time.deltaTime);
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 forwardVel = -transform.up * Vector2.Dot(rb.linearVelocity, -transform.up);
        Vector2 sidewaysVel = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);

        if (driftingLeft)
        {
            rb.linearVelocity = forwardVel + sidewaysVel * driftSlide;
        }
        else if (driftingRight)
        {
            rb.linearVelocity = forwardVel + sidewaysVel * driftSlide;
        }
        else
        {
            rb.linearVelocity = forwardVel + sidewaysVel * driftFactor;
        }
    }
}
