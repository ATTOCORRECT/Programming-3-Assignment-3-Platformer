using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection { Left, Right }
    public FacingDirection direction;

    public float topSpeed;
    
    bool up, left, down, right, jump, crouch, special; //input

    Rigidbody2D rigidbody;

    Vector2[] corners = { new Vector2(-4f / 16f, -8f / 16f),
                          new Vector2( 4f / 16f, -8f / 16f),
                          new Vector2( 4f / 16f,  4f / 16f),
                          new Vector2(-4f / 16f,  4f / 16f)};

    int maxBounces = 5;
    float skinWidth = 0.01f;

    Vector2 velocity;
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();

        velocity = Vector2.left * Time.fixedDeltaTime;
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

    public bool IsGrounded()
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

        //collision box debug
        Debug.DrawLine(rigidbody.position + corners[0], rigidbody.position + corners[1]);
        Debug.DrawLine(rigidbody.position + corners[1], rigidbody.position + corners[2]);
        Debug.DrawLine(rigidbody.position + corners[2], rigidbody.position + corners[3]);
        Debug.DrawLine(rigidbody.position + corners[3], rigidbody.position + corners[0]);
    }

    void PlayerMovement()
    {
        


        //Gravity();
        velocity += WalkVelocity();
        //Collisions
        velocity = CollideAndSlide(velocity, rigidbody.position, 0, false);
        velocity += CollideAndSlide(Vector2.down * Time.fixedDeltaTime * 0.1f, rigidbody.position, 0, true);
        rigidbody.position += velocity ;
    }

    Vector2 CollideAndSlide(Vector2 velocity, Vector2 position, int depth, bool gravityPass)
    {
        if(depth >= maxBounces)
        {
            return Vector2.zero;
        }

        float distance = velocity.magnitude + skinWidth;

        for (int i = 0; i < 4; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(position + corners[i], velocity.normalized, distance, LayerMask.GetMask("Ground"));
            //Debug.DrawRay(position + corners[i], velocity, Color.white);

            if (hit)
            {
                Vector2 snapToSurface = velocity.normalized * (hit.distance - skinWidth);
                Vector2 leftover = velocity - snapToSurface;

/*                if(snapToSurface.magnitude <= skinWidth)
                {
                    snapToSurface = Vector2.zero;
                }*/

                float mag = leftover.magnitude;
                leftover = Vector3.ProjectOnPlane(leftover, hit.normal).normalized;
                leftover *= mag;

                return snapToSurface + CollideAndSlide(leftover, position + snapToSurface, depth + 1, gravityPass);
            }
        }

        return velocity;
    }

    Vector2 WalkVelocity()
    {
        Vector2 walkVelocity = Vector2.zero;

        Debug.Log(velocity.x / Time.fixedDeltaTime);

        if (left  && velocity.x / Time.fixedDeltaTime > -1) { walkVelocity += Vector2.left  * 1; }
        if (right && velocity.x / Time.fixedDeltaTime <  1) { walkVelocity += Vector2.right * 1; }

        return walkVelocity * Time.fixedDeltaTime;
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
