using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Controller : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnAngle = 10f;

    void Update()
    {
        // forward movement
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.Self);

        // turning
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0f, 0f, turnAngle * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0f, 0f, -turnAngle * Time.deltaTime);
        }
    }
}
