using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection { Left, Right }
    public float topSpeed;
    public FacingDirection direction;
    bool up, left, down, right, jump, crouch, special;
    Rigidbody2D rigidbody;

    Vector2 velocity = Vector2.zero;
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }


    public bool IsWalking()
    {
        return false;
    }

    public bool CanJump()
    {
        float distanceAboveGround = (10f / 16f);
        bool grounded = Physics2D.Raycast(transform.position, Vector2.down, distanceAboveGround, LayerMask.GetMask("Ground"));
        return grounded;
    }

    public FacingDirection GetFacingDirection()
    {
        return direction;
    }

    private void Update()
    {
        PlayerInput();
        UpdateDirection();

    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    void PlayerMovement()
    {
        Gravity();



        GroundCollision();


        rigidbody.position += velocity * Time.fixedDeltaTime;
    }

    void GroundCollision()
    {
        Vector2 step = velocity * Time.fixedDeltaTime;

        Debug.DrawRay(rigidbody.position - Vector2.one / 2, Vector2.right);
        Debug.DrawRay(rigidbody.position - Vector2.one / 2, Vector2.up);
        Debug.DrawRay(rigidbody.position + Vector2.one / 2, Vector2.left);
        Debug.DrawRay(rigidbody.position + Vector2.one / 2, Vector2.down);

        RaycastHit2D groundCollision = Physics2D.CircleCast(rigidbody.position, 0.5f, step, step.magnitude, LayerMask.GetMask("Ground"));

        Debug.DrawRay(rigidbody.position, step);
        Debug.DrawRay(rigidbody.position - Vector2.one/2 + step, Vector2.right);

        if (groundCollision)
        {
            Debug.DrawRay(groundCollision.point + Vector2.left / 4, Vector2.right / 2, Color.cyan);
            Debug.DrawRay(groundCollision.point + Vector2.up / 4, Vector2.down / 2, Color.cyan);

            Vector2 newStep = Vector2.ClampMagnitude(step, groundCollision.distance);

            Vector2 leftoverStep = step - newStep;

            //Vector2 slideStep = << HERE (perp, swap x & y )

            velocity = newStep / Time.fixedDeltaTime;
        }
    }

    Vector2 Project(Vector2 a, Vector2 b)
    {
        return (Vector2.Dot(b, a) / b.magnitude * b.magnitude) * b;
    }

    void Gravity()
    {
        float terminalVelocity = 20; // units per second
        float gravity = 20; // units per second per second

        if (velocity.y > -terminalVelocity)
        {
            velocity += Vector2.down * gravity * Time.fixedDeltaTime;
        }
        else if (velocity.y < -terminalVelocity)
        {
            Vector3 currentVel = velocity;
            velocity = new Vector2 (currentVel.x, -terminalVelocity);
        }
        
    }

    void UpdateDirection()
    {
        if (left)  { direction = FacingDirection.Left; }
        if (right) { direction = FacingDirection.Right; }
    }
    void PlayerInput()
    {
        up     = Input.GetKey(KeyCode.W);
        left   = Input.GetKey(KeyCode.A);
        down   = Input.GetKey(KeyCode.S);
        right  = Input.GetKey(KeyCode.D);

        jump   = Input.GetKey(KeyCode.Space);
        crouch = Input.GetKey(KeyCode.LeftControl);

        //special = Input.GetKey(KeyCode.LeftShift);

    }
}
