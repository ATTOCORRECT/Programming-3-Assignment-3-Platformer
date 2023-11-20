using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection { Left, Right }
    public FacingDirection direction;

    public float topSpeed, slideSpeed, airSpeed;

    public GameObject spriteObject;
    Vector2 spriteScale = Vector2.one;

    public BoxCollider2D collisionBox;
    Vector2 collisionBoxDefaultSize, collisionBoxCrouchedSize, collisionBoxDefaultOffset, collisionBoxCrouchedOffset;

    LayerMask ground;

    public enum State { Default, Slide }
    State state;

    bool up, left, down, right, jump, special; //input

    float inputX = 0;

    float pixel = 1;

    Vector2 position, velocity, remainder;

    private void Start()
    {
        collisionBoxDefaultSize = new Vector2(pixel * 8, pixel * 12);
        collisionBoxCrouchedSize = new Vector2(pixel * 8, pixel * 6);
        collisionBoxDefaultOffset = new Vector2(0, -pixel * 2);
        collisionBoxCrouchedOffset = new Vector2(0, -pixel * 5);

        ground = LayerMask.GetMask("Ground");

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
        return collideAt(ground, Vector2.down);
    }

    public bool IsRiding(GameObject solid)
    {
        RaycastHit2D Hit = collideAt(ground, Vector2.down);
        if (Hit)
        {
            return solid == Hit.collider.gameObject;
        }
        return false;
    }

    public State GetState()
    {
        return state;
    }

    public FacingDirection GetFacingDirection()
    {
        return direction;
    }

    private void Update()
    {
        UpdateDirection();
        PlayerInput();

        spriteObject.transform.localScale = new Vector2(Mathf.Round(spriteScale.x * 8) / 8, Mathf.Round(spriteScale.y * 8) / 8);
    }

    private void FixedUpdate()
    {
        Physics();

        //Debug.Log("Velocity " + velocity);

        //Debug.Log("is Grounded " + IsGrounded());

        //Debug.Log("is Walking " + IsWalking());

        //Debug.Log("State " + state);

    }

    void Physics()
    {
        //VelocityInheritance();

        Default();
        Slide();
        Walk();
        Jump();
        AirControll();
        GroundFriction();
        AirResistance();

        velocity += Vector2.down * 0.2f; // gravity

        MoveX(velocity.x);
        MoveY(velocity.y);
    }

    RaycastHit2D collideAt(LayerMask layermask, Vector2 offset)
    {
        Vector2 rayOrigin = position + collisionBox.offset;
        Vector2 rayDirection = offset;
        float rayDistance = offset.magnitude;

        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin, collisionBox.size - Vector2.one, 0, rayDirection, rayDistance, layermask);

        Debug.DrawRay(rayOrigin, rayDirection.normalized * rayDistance, Color.red);
        return hit;
    }

    public void MoveX(float amount)
    {
        remainder.x += amount;
        int move = Mathf.RoundToInt(remainder.x);

        if (move != 0)
        {
            remainder.x -= move;
            int sign = (int)Mathf.Sign(move);

            {
                while (move * sign > 0)
                {
                    if(!collideAt(ground, new Vector2(sign, 0)))
                    {
                        // no ground immediately beside
                        position.x += sign;
                        move -= sign;
                    }
                    else
                    {
                        // hit ground!
                        //Debug.Log("Horizontal Collision!");
                        velocity.x = 0;
                        break;
                    }
                }
            }
        }
        transform.position = position;
    } 

    public void MoveY(float amount)
    {
        remainder.y += amount;
        int move = Mathf.RoundToInt(remainder.y);

        if (move != 0)
        {
            remainder.y -= move;
            int sign = (int)Mathf.Sign(move);

            while (move * sign > 0)
            {
                if (!collideAt(ground, new Vector2(0, sign)))
                {
                    // no ground immediately beside
                    position.y += sign;
                    move -= sign;
                }
                else
                {
                    // hit ground!
                    //Debug.Log("Vertical Collision!");
                    velocity.y = 0;
                    break;
                }
            }
        }
        transform.position = position;
    }

    void Default()
    {
        if (state == State.Default)
        {
            spriteScale = Vector2.Lerp(spriteScale, Vector2.one, 0.25f);
            collisionBox.size = collisionBoxDefaultSize;
            collisionBox.offset = collisionBoxDefaultOffset;
        }
    }

    void Slide()
    {
        if (down && IsGrounded() && state == State.Default)
        {
            state = State.Slide;

            spriteScale = new Vector2(1.5f, 0.5f);

            collisionBox.size = collisionBoxCrouchedSize;
            collisionBox.offset = collisionBoxCrouchedOffset;

            velocity += Mathf.Clamp(-velocity.x * inputX + slideSpeed, 0, slideSpeed) * inputX * Vector2.right;
        }

        if ((!down || !IsGrounded()) && state == State.Slide)
        {
            state = State.Default;
        }

        if (state == State.Slide)
        {
            spriteScale = Vector2.Lerp(spriteScale, Vector2.one, 0.25f);
            velocity += Mathf.Clamp(-velocity.x * inputX + slideSpeed, 0, slideSpeed * 0.01f) * inputX * Vector2.right;
        }
    }

    void Walk()
    {
        if (IsGrounded() && state == State.Default)
        {
            velocity += Mathf.Clamp(-velocity.x * inputX + topSpeed, 0, topSpeed * 0.1f) * inputX * Vector2.right;
        }
    }

    void Jump()
    {
        if (IsGrounded() && jump)
        {
            spriteScale = new Vector2(0.5f, 1.5f);
            velocity.y = 4;
        }
    }
    void AirControll()
    {
        if (!IsGrounded())
        {
            velocity += Mathf.Clamp(-velocity.x * inputX + airSpeed, 0, airSpeed * 0.05f) * inputX * Vector2.right;
        }
    }

    void GroundFriction()
    {
        if ((inputX == 0 || velocity.x * Mathf.Sign(inputX) > topSpeed ) && IsGrounded() && state == State.Default)
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, 0.25f);
        }
    }

    void AirResistance()
    {
        if (inputX * Mathf.Sign(velocity.x) <= 0 && velocity.x * Mathf.Sign(inputX) < airSpeed * 1.5f && !IsGrounded() && state == State.Default)
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, 0.05f);
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
        left  = Input.GetKey(KeyCode.A); // move left
        down  = Input.GetKey(KeyCode.S); // slide
        right = Input.GetKey(KeyCode.D); // move right

        jump   = Input.GetKey(KeyCode.Space); // jump

        //special = Input.GetKey(KeyCode.LeftShift);

        inputX = 0;
        if (left) { inputX -= 1; }
        if (right) { inputX += 1; }
    }
}
