using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection { Left, Right }
    public FacingDirection direction;

    public float topSpeed;
    public float slideSpeed;
    public BoxCollider2D collisionBox;
    Vector2 collisionBoxDefaultSize, collisionBoxCrouchedSize, collisionBoxDefaultOffset, collisionBoxCrouchedOffset;

    enum State { Default, Slide }
    State state;

    bool up, left, down, right, jump, special; //input

    float pixel = (1f / 16f);

    Vector2 position, velocity, additionalRelativeVelocity;

    private void Start()
    {
        collisionBoxDefaultSize = new Vector2(pixel * 8, pixel * 12);
        collisionBoxCrouchedSize = new Vector2(pixel * 8, pixel * 6);
        collisionBoxDefaultOffset = new Vector2(0, -pixel * 2);
        collisionBoxCrouchedOffset = new Vector2(0, -pixel * 5);

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
            Vector2 rayOrigin = position + collisionBox.offset + (Mathf.Lerp(collisionBox.size.x - 0.0001f, -collisionBox.size.x + 0.0001f, i / (float)(rays - 1)) * Vector2.right / 2);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, collisionBox.size.y / 2, LayerMask.GetMask("Ground"));
            Debug.DrawRay(rayOrigin, Vector2.down * (collisionBox.size.y / 2 + pixel), Color.cyan);
            if (hit) { return true; }
        }
        return false;
    }

    public GameObject Touching()
    {
        int rays = 3;
        for (int i = 0; i < rays; i++)
        {
            Vector2 rayOrigin = position + collisionBox.offset + (Mathf.Lerp(collisionBox.size.x - 0.0001f, -collisionBox.size.x + 0.0001f, i / (float)(rays - 1)) * Vector2.right / 2);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, collisionBox.size.y / 2 + pixel * 8, LayerMask.GetMask("Ground"));
            if (hit) { return hit.collider.gameObject; }
        }
        return null;
    }

    public FacingDirection GetFacingDirection()
    {
        return direction;
    }

    private void Update()
    {
        UpdateDirection();
        PlayerInput();
    }

    private void FixedUpdate()
    {
        Physics();
        //Debug.Log("Velocity " + velocity);

        //Debug.Log("is Grounded " + IsGrounded());

        //Debug.Log("is Walking " + IsWalking());

        //Debug.Log("State " + state);

        Debug.Log(Touching() != null);

    }

    void Physics()
    {
        velocity += Vector2.down * 0.01f; // gravity

        //collision checks
        CollideH();
        CollideV();

        position += velocity;
        transform.position = position;

        VelocityInheritance();

        Default();
        Slide();
        Walk();
        GroundFriction();
    }

    void CollideH()
    {
        int rays = 3;
        for (int i = 0; i < rays; i++)
        {
            Vector2 rayOrigin = position + collisionBox.offset + (Mathf.Lerp(collisionBox.size.y - 0.0001f, -collisionBox.size.y + 0.0001f, i / (float)(rays - 1)) * Vector2.up / 2);
            Vector2 rayDirection = velocity.x * Vector2.right;
            float offset = collisionBox.size.x / 2 * Mathf.Sign(Mathf.Abs(velocity.x));
            float rayDistance = Mathf.Abs(velocity.x) + offset;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, LayerMask.GetMask("Ground"));
            Debug.DrawRay(rayOrigin, rayDirection.normalized * rayDistance, Color.red);

            if (hit)
            {
                //if (hit.distance < offset) { GameObject.Destroy(gameObject); }
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
            Vector2 rayOrigin = position + collisionBox.offset + (Mathf.Lerp(collisionBox.size.x - 0.0001f, -collisionBox.size.x + 0.0001f, i / (float)(rays - 1)) * Vector2.right / 2);
            Vector2 rayDirection = velocity.y * Vector2.up;
            float offset = collisionBox.size.y / 2 * Mathf.Sign(Mathf.Abs(velocity.y));
            float rayDistance = Mathf.Abs(velocity.y) + offset;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, LayerMask.GetMask("Ground"));
            Debug.DrawRay(rayOrigin, rayDirection.normalized * rayDistance, Color.red);

            if (hit)
            {
                //if (hit.distance < offset) { GameObject.Destroy(gameObject); }
                velocity = new Vector2(velocity.x, Mathf.Clamp(velocity.y, -hit.distance + offset, hit.distance - offset));
                return;
            }
        }
    }

    void Default()
    {
        if (state == State.Default)
        {
            collisionBox.size = collisionBoxDefaultSize;
            collisionBox.offset = collisionBoxDefaultOffset;
        }
    }

    void Slide()
    {
        if (down && IsGrounded() && state == State.Default)
        {
            state = State.Slide;

            if (left)
            {
                velocity += Mathf.Clamp(-Mathf.Abs(velocity.x) + slideSpeed + Mathf.Abs(additionalRelativeVelocity.x), 0, slideSpeed) * Vector2.left;
            }

            if (right)
            {
                velocity += Mathf.Clamp(-Mathf.Abs(velocity.x) + slideSpeed + Mathf.Abs(additionalRelativeVelocity.x), 0, slideSpeed) * Vector2.right;
            }
        }

        if ((!down || !IsGrounded()) && state == State.Slide)
        {
            state = State.Default;
        }

        if (state == State.Slide)
        {
            collisionBox.size = collisionBoxCrouchedSize;
            collisionBox.offset = collisionBoxCrouchedOffset;
        }
    }

    void Walk()
    {
        if (IsGrounded() && state == State.Default)
        {
            if (left)
            {
                velocity += Mathf.Clamp(-velocity.x + topSpeed + Mathf.Abs(additionalRelativeVelocity.x), 0, topSpeed * 0.1f) * Vector2.left;
            }

            if (right)
            {
                velocity += Mathf.Clamp(velocity.x + topSpeed + Mathf.Abs(additionalRelativeVelocity.x), 0, topSpeed * 0.1f) * Vector2.right;
            }
        }
    }

    void VelocityInheritance()
    {
        additionalRelativeVelocity = Vector2.zero;
        if (Touching() != null)
        {
            if (Touching().tag == "MovingPlatform")
            {
                additionalRelativeVelocity = Touching().GetComponent<MovingPlatform>().getVelocity();
                velocity += Touching().GetComponent<MovingPlatform>().getAcceleration();
            }
        }
    }

    void GroundFriction()
    {
        Vector2 targetVelocity = Vector2.zero;

        if ((!(left != right) || Mathf.Abs(velocity.x) > topSpeed + Mathf.Abs(additionalRelativeVelocity.x)) && IsGrounded() && state == State.Default)
        {
            if (Touching() != null)
            {
                if (Touching().tag == "MovingPlatform")
                {
                    targetVelocity = Touching().GetComponent<MovingPlatform>().getVelocity();
                }
            }

            velocity = Vector2.Lerp(velocity, targetVelocity, 0.25f);
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
        //up    = Input.GetKey(KeyCode.W);
        left  = Input.GetKey(KeyCode.A);
        down  = Input.GetKey(KeyCode.S); // slide
        right = Input.GetKey(KeyCode.D);

        jump   = Input.GetKey(KeyCode.Space);

        //special = Input.GetKey(KeyCode.LeftShift);
    }

    public void Push(Vector2 push)
    {
        position += push;
    }
}
