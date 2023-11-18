using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection { Left, Right }
    public FacingDirection direction;

    public float topSpeed;

    public BoxCollider2D collisionBox;

    bool up, left, down, right, jump, crouch, special; //input

    float pixel = (1f / 16f);

    Vector2 position, velocity;

    private void Start()
    {
        velocity = Vector2.zero;
        position = transform.position;
    }

    public bool IsWalking()
    {
        if (IsGrounded() && (left || right) && Mathf.Abs(velocity.x) > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsGrounded()
    {
        int rays = 3;
        for (int i = 0; i < rays; i++)
        {
            Vector2 rayOrigin = position + collisionBox.offset + (Mathf.Lerp(collisionBox.size.x - pixel, -collisionBox.size.x + pixel, i / (float)(rays - 1)) * Vector2.right / 2);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, collisionBox.size.y / 2, LayerMask.GetMask("Ground"));
            Debug.DrawRay(rayOrigin, Vector2.down * (collisionBox.size.y / 2 + pixel), Color.cyan);
            if (hit) { return true; }
        }
        return false;
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
        Physics();

        Debug.Log("Velocity " + velocity);

        Debug.Log("is Grounded " + IsGrounded());

        Debug.Log("is Walking " + IsWalking());
    }

    void Physics()
    {
        position += velocity;
        transform.position = position;

        Walk();
        GroundFriction();
        velocity += Vector2.down * 0.01f;

        CollideH();
        CollideV();
    }

    void CollideH()
    {
        int rays = 3;
        for (int i = 0; i < rays; i++)
        {
            Vector2 rayOrigin = position + collisionBox.offset + (Mathf.Lerp(collisionBox.size.y - pixel, -collisionBox.size.y + pixel, i / (float)(rays - 1)) * Vector2.up / 2);
            Vector2 rayDirection = velocity.x * Vector2.right;
            float offset = collisionBox.size.x / 2 * Mathf.Sign(Mathf.Abs(velocity.x));
            float rayDistance = Mathf.Abs(velocity.x) + offset;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, LayerMask.GetMask("Ground"));
            Debug.DrawRay(rayOrigin, rayDirection.normalized * rayDistance, Color.red);

            if (hit)
            {
                velocity = new Vector2(Mathf.Clamp(velocity.x, -hit.distance + offset, hit.distance - offset), velocity.y);
                return;
            }
        }
    }

    void CollideV()
    {
        int rays = 3;
        for (int i = 0; i < rays; i++)
        {
            Vector2 rayOrigin = position + collisionBox.offset + (Mathf.Lerp(collisionBox.size.x - pixel, -collisionBox.size.x + pixel, i / (float)(rays - 1)) * Vector2.right / 2);
            Vector2 rayDirection = velocity.y * Vector2.up;
            float offset = collisionBox.size.y / 2 * Mathf.Sign(Mathf.Abs(velocity.y));
            float rayDistance = Mathf.Abs(velocity.y) + offset;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, LayerMask.GetMask("Ground"));
            Debug.DrawRay(rayOrigin, rayDirection.normalized * rayDistance, Color.red);

            if (hit)
            {
                velocity = new Vector2(velocity.x, Mathf.Clamp(velocity.y, -hit.distance + offset, hit.distance - offset));
                return;
            }
        }
    }

    void Walk()
    {
        if (IsGrounded())
        {
            if (left)
            {
                velocity += Mathf.Clamp(velocity.x + topSpeed, 0, topSpeed * 0.1f) * Vector2.left;
            }

            if (right)
            {
                velocity += Mathf.Clamp(-velocity.x + topSpeed, 0, topSpeed * 0.1f) * Vector2.right;
            }
        }
    }

    void GroundFriction()
    {
        if (!(left != right) && IsGrounded())
        {
            velocity *= 0.75f;
        }
    }

    void UpdateDirection()
    {
        if (!(left && right))
        {
            if (left) { direction = FacingDirection.Left; }
            if (right) { direction = FacingDirection.Right; }
        }
    }
    void PlayerInput()
    {
        up    = Input.GetKey(KeyCode.W);
        left  = Input.GetKey(KeyCode.A);
        down  = Input.GetKey(KeyCode.S);
        right = Input.GetKey(KeyCode.D);

        jump   = Input.GetKey(KeyCode.Space);
        //crouch = Input.GetKey(KeyCode.LeftControl); // maybe no crouching?

        special = Input.GetKey(KeyCode.LeftShift);
    }
}
