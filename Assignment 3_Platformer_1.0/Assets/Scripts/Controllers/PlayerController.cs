using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection { Left, Right }
    public FacingDirection direction;

    public float walkSpeed, walkAcceleration, slideSpeed, airSpeed, airAcceleration, jumpVelocity;
    float gravity = 0.2f;

    public GameObject spriteObject;
    Vector2 spriteScale = Vector2.one;

    public BoxCollider2D collisionBox;
    Vector2 collisionBoxDefaultSize, collisionBoxCrouchedSize, collisionBoxDefaultOffset, collisionBoxCrouchedOffset;

    LayerMask ground, semiSolid;

    public enum State { Default, Slide, WallSlide }
    State state;

    bool up, left, down, right, jump, special; //input
    int jumpBuffer = 0;
    int inputX = 0;

    float pixel = 1;

    [System.NonSerialized]
    public Vector2 position, velocity, remainder, inheritedVelocity;

    private void Start()
    {
        collisionBoxDefaultSize = new Vector2(pixel * 8, pixel * 12);
        collisionBoxCrouchedSize = new Vector2(pixel * 8, pixel * 6);
        collisionBoxDefaultOffset = new Vector2(0, -pixel * 2);
        collisionBoxCrouchedOffset = new Vector2(0, -pixel * 5);

        ground = LayerMask.GetMask("Ground");
        semiSolid = LayerMask.GetMask("Semi Solid");

        velocity = Vector2.zero;
        inheritedVelocity = Vector2.zero;
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

        spriteObject.transform.localScale = new Vector2(Mathf.Round(spriteScale.x * 8) / 8, Mathf.Round(spriteScale.y * 8) / 8); // pixel perfect!
    }

    private void FixedUpdate()
    {
        Physics();
        UpdateInput();

        //Debug.Log("Velocity " + velocity);

        //Debug.Log("is Grounded " + IsGrounded());

        //Debug.Log("is Walking " + IsWalking());

        //Debug.Log("State " + state);
    }

    void Physics()
    {
        Default();
        Slide();
        WallSlide();
        Walk();
        Jump();
        AirControll();
        GroundFriction();
        AirResistance();

        velocity += Vector2.down * gravity; // gravity

        // movement and collision handling
        MoveX(velocity.x);
        MoveY(velocity.y);

        inheritedVelocity = Vector2.zero; // clear residual inherited velocity

        Squish();
    }

    RaycastHit2D collideAt(LayerMask layermask, Vector2 offset) // checks for a collision in the defined offset from player center and layermask
    {
        Vector2 rayOrigin = position + collisionBox.offset;
        Vector2 rayDirection = offset;
        float rayDistance = offset.magnitude;

        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin, collisionBox.size - Vector2.one, 0, rayDirection, rayDistance, layermask);

        Debug.DrawRay(rayOrigin, rayDirection.normalized * rayDistance, Color.red);
        return hit;
    }

    public void MoveX(float amount) // move player in the X axis
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

                        // if moving fast enough and pressing a directional key, convert horizontal vel to vertical vel
                        if (Mathf.Abs(velocity.x) > 2 && inputX != 0) 
                        {
                            float convertedVelocity = Mathf.Abs(velocity.x);
                            velocity.x = 0;
                            velocity.y = convertedVelocity;
                        }
                        else
                        {
                            velocity.x = 0;
                        }
                        break;
                    }
                }
            }
        }
        transform.position = position;
    }

    /* btw my justification for handling movement & collisions like this is that ive tried 2 other collision algorithms, didnt like 
     * them for what i was trying to achive, and decided that it was best that if i wanted to make a pixel precision type platformer
     * that i should just simply follow what celeste did to make theirs. one thing led to the next and i found documentation and 
     * a blog post from maddy (dev for celeste) explaining how the collision handling works */

    public void MoveY(float amount) // move player in the Y axis
    {
        remainder.y += amount;
        int move = Mathf.RoundToInt(remainder.y);

        if (move != 0)
        {
            remainder.y -= move;
            int sign = (int)Mathf.Sign(move);

            while (move * sign > 0)
            {
                // normal ground
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

                //semi solid platform
            }
        }
        transform.position = position;
    }

    void Default() // default state settigns
    {
        if (state == State.Default)
        {
            spriteScale = Vector2.Lerp(spriteScale, Vector2.one, 0.25f); // return sprite to normal

            // normal hitbox
            collisionBox.size = collisionBoxDefaultSize;
            collisionBox.offset = collisionBoxDefaultOffset;
        }
    }

    void Slide() // handles slide movement ability
    {
        if (down && IsGrounded() && state == State.Default) // start slide
        {
            Crouch();

            velocity += Mathf.Clamp(-velocity.x * inputX + slideSpeed, 0, slideSpeed) * inputX * Vector2.right; // capped impulse boost
        }

        bool headRoom = !collideAt(ground, Vector2.up * 6 * pixel);
        if ((!down || !IsGrounded()) && state == State.Slide && headRoom) // stop slide
        {
            state = State.Default;
        }

        if (state == State.Slide) // sliding
        {
            spriteScale = Vector2.Lerp(spriteScale, Vector2.one, 0.25f); // return sprite to normal
            velocity += Mathf.Clamp(-velocity.x * inputX + slideSpeed, 0, slideSpeed * 0.01f) * inputX * Vector2.right;
        }
    }

    void WallSlide() // handles wall slide movement ability
    {
        bool IsTouchingWall = collideAt(ground, Vector2.left) || collideAt(ground, Vector2.right);
        if(!IsGrounded() && IsTouchingWall)
        {
            if (velocity.y > 0) // up
            {
                if (BufferedJumpInput()) // wall jump up
                {
                    spriteScale = new Vector2(0.5f, 1.5f); // stretch
                    velocity.y += jumpVelocity / 2;
                    velocity.x = slideSpeed * -directionOfWall();
                }
            }

            if (velocity.y < 0) // down
            {
                if (inputX == directionOfWall()) // slide down
                {
                    velocity.y = Mathf.Lerp(velocity.y, 0, 0.25f);
                }

                if (BufferedJumpInput()) // wall jump away
                {
                    spriteScale = new Vector2(0.5f, 1.5f); // stretch
                    velocity.x = slideSpeed * -directionOfWall();
                }
            }
        }
    }
    int directionOfWall() // returns wether the contacting wall is to the left (-1) or to the right (1)
    {
        int direction = 0;
        bool left = collideAt(ground, Vector2.left);
        bool right = collideAt(ground, Vector2.right);

        if (left) { direction -= 1; }
        if (right) { direction += 1; }

        return direction;
    }

    void Crouch() // crouches the player
    {
        state = State.Slide;

        spriteScale = new Vector2(1.5f, 0.5f); // squish

        // crouched hitbox
        collisionBox.size = collisionBoxCrouchedSize;
        collisionBox.offset = collisionBoxCrouchedOffset;
    }

    void Walk() // handles walking
    {
        if (IsGrounded() && state == State.Default)
        {
            velocity += Mathf.Clamp(-velocity.x * inputX + walkSpeed, 0, walkSpeed * walkAcceleration) * inputX * Vector2.right; // capped acceleration
        }
    }

    void Jump() // handles jumping
    {
        if (IsGrounded() && BufferedJumpInput())
        {
            spriteScale = new Vector2(0.5f, 1.5f); // stretch

            velocity.y = jumpVelocity + inheritedVelocity.y;
            velocity.x += inheritedVelocity.x;

        }
    }

    void AirControll() // handles air controll
    {
        if (!IsGrounded())
        {
            velocity += Mathf.Clamp(-velocity.x * inputX + airSpeed, 0, airSpeed * airAcceleration) * inputX * Vector2.right; // capped acceleration
        }
    }

    void GroundFriction() // handles ground friction
    {
        // if ( no x input or x velocity is greater than walk speed ) && grounded && default state
        if ((inputX == 0 || Mathf.Abs(velocity.x) > walkSpeed) && IsGrounded() && state == State.Default)
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, 0.25f);
        }
    }

    void AirResistance() // handles air resistance
    {
        // player is trying to resist movement or isnt moving && they havent hit critical airspeed (no resistance) && is in the air && default state
        // if the input dir doesnt match the velocity dir && horizontal velocity is less than 1.5x the max airspeed && not grounded && default state
        if (inputX * Mathf.Sign(velocity.x) <= 0 && velocity.x * Mathf.Sign(inputX) < airSpeed * 1.5f && !IsGrounded() && state == State.Default) // this line is kinda hard to follow, hopefully the 2 explinations help
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, 0.05f);
        }
    }

    void Squish()
    {
        if (collideAt(ground, Vector2.zero) && state == State.Slide)
        {
            GameObject.Destroy(gameObject);
        }

        if (collideAt(ground, Vector2.zero))
        {
            Crouch();
        }
    }

    void UpdateDirection()
    {
        // left XNOR right
        if (left != right)
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

        if (!jump)
        {
            jump = Input.GetKeyDown(KeyCode.Space); // jump
        }
        
        //special = Input.GetKey(KeyCode.LeftShift);

        inputX = 0; // x input as an int. -1 left, 0 nothing, 1 right.
        if (left) { inputX -= 1; }
        if (right) { inputX += 1; }
    }

    bool BufferedJumpInput()
    {
        if (jumpBuffer > 0)
        {
            jumpBuffer = 0;
            return true;
        }
        return false;
    }

    void UpdateInput()
    {
        if (jump) { jumpBuffer = 10; }
        if (jumpBuffer > 0) { jumpBuffer -= 1; }

        jump = false;
    }

    public void SetInheritedVelocity(Vector2 inheritedVelocity)
    {
        this.inheritedVelocity = inheritedVelocity;
    }
}
