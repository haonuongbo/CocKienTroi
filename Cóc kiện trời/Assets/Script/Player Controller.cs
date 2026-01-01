using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float torqueAmount = 10f;
    public float basespeed;
    public float boostSpeed;

    Rigidbody2D RGB;

    SurfaceEffector2D surfaceEffector2D;

    bool canMove = true;
    void Start()
    {
         RGB = GetComponent<Rigidbody2D>();
        surfaceEffector2D = Object.FindFirstObjectByType<SurfaceEffector2D>();
    }

    void RotatePlayer()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            RGB.AddTorque(torqueAmount);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            RGB.AddTorque(-torqueAmount);
        }
    }
    void RespondToBoost()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            surfaceEffector2D.speed = boostSpeed;
        }
        else
        {
            surfaceEffector2D.speed = basespeed;
        }
    }
    void Update()
    {
        if(canMove)
        {
            RotatePlayer();
            RespondToBoost();
        }
     
    }
    public void DisableControls()
    {
        canMove = false;
    }
}
