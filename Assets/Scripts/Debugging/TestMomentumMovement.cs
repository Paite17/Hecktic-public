using UnityEngine;

public class TestMomentumMovement : MonoBehaviour
{
    public float speed = 5f;
    public float acceleration = 5f;
    public float deceleration = 5f;
    public float maxSpeed = 10f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 moveVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get input from horizontal axis (A/D or Left/Right arrow keys)
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Normalize input to prevent faster diagonal movement
        moveInput = moveInput.normalized;

        // Calculate target velocity based on input
        moveVelocity = moveInput * speed;
    }

    void FixedUpdate()
    {
        if (moveInput != Vector2.zero)
        {
            // Accelerate towards target velocity
            rb.velocity = Vector2.MoveTowards(rb.velocity, moveVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate to a stop when no input is detected
            rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // Clamp velocity to maximum speed
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
    }
}
