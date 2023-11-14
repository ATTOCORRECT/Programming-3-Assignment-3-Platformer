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
    CircleCollider2D collider;

    float skinWidth = 0.01f;
    int maxBounces = 5;
    Bounds bounds;

    Vector2 velocity = Vector2.down;
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();
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
        Walk();


        //Gravity();


        //GroundCollision();


        //Collisions

        bounds = collider.bounds;
        bounds.Expand(-2 * skinWidth);

        velocity =  CollideAndSlide(velocity, rigidbody.position, 0, false);
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

        RaycastHit2D hit = Physics2D.CircleCast(position, bounds.extents.x, velocity.normalized, distance, LayerMask.GetMask("Ground"));
        if (hit)
        {
            Vector2 snapToSurface = velocity.normalized * (hit.distance - skinWidth);
            Vector2 leftover = velocity - snapToSurface;

            if(snapToSurface.magnitude <= skinWidth)
            {
                snapToSurface = Vector2.zero;
            }

            float mag = leftover.magnitude;
            leftover = Vector3.ProjectOnPlane(leftover, hit.normal).normalized;
            leftover *= mag;

            return snapToSurface + CollideAndSlide(leftover, position + snapToSurface, depth + 1, gravityPass);
        }

        return velocity;
    }

    void Walk()
    {
       
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
