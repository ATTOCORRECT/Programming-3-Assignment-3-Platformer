using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection { Left, Right }
    public FacingDirection direction;

    public float walkSpeed, walkAcceleration, crouchSpeed, airSpeed, airAcceleration, jumpVelocity, variableJumpSpeed;
    float gravity = 0.2f;

    public GameObject spriteObject;
    Vector2 spriteScale = Vector2.one;

    public BoxCollider2D collisionBox;
    Vector2 collisionBoxDefaultSize, collisionBoxCrouchedSize, collisionBoxDefaultOffset, collisionBoxCrouchedOffset;

    LayerMask ground;//, semiSolid;

    public enum AbilityState { Default, Crouch }
    AbilityState abilityState;
    public enum GraphicsState { Idle, Crouch, Jump, WallSlide, Walking , CrouchWalking }
    GraphicsState graphicsState;

    bool up, left, down, right, jump, jumping, special; //input
    int jumpBuffer = 0, groundCoyoteCounter = 0, variableJumpTimer = 0, wallCoyoteCounter = 0;
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

        velocity = Vector2.zero;
        inheritedVelocity = Vector2.zero;
        position = transform.position;
    }

    public bool IsWalking()
    {
        if (IsGrounded() && (inputX != 0 || velocity.x != 0))
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

    public bool IsTouchingWall()
    {
        return collideAt(ground, Vector2.right) || collideAt(ground, Vector2.left);
    }

    public bool IsCoyoteGrounded()
    {
        if (groundCoyoteCounter > 0)
        {
            return true;
        }
        return false;
    }
    public bool IsWallCoyoteGrounded()
    {
        if (wallCoyoteCounter > 0)
        {
            return true;
        }
        return false;
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

    public GraphicsState GetGraphicsState()
    {
        return graphicsState;
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
        UpdateCoyote();
        UpdateWallCoyote();

        UpdateGraphics();
        //Debug.Log("Velocity " + velocity);

        //Debug.Log("is Grounded " + IsGrounded());

        //Debug.Log("is Walking " + IsWalking());

        //Debug.Log("State " + state);

        //Debug.Log(groundCoyoteCounter);
    }

    void Physics()
    {
        Default();
        CrouchAbility();
        Walk();
        WallSlide();
        VariableJump();
        Jump();
        WallJump();
        AirControll();
        GroundFriction();
        AirResistance();
        Gravity();
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
                        velocity.x = 0;
                        break;
                    }
                }
            }
        }
        transform.position = position;
    }

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
                if (!collideAt(ground, new Vector2(0, sign)))
                {
                    // no ground immediately beside
                    position.y += sign;
                    move -= sign;
                }
                else
                {
                    // hit ground!
                    velocity.y = 0;
                    break;
                }

                //semi solid platform
            }
        }
        transform.position = position;
    }

    /* btw my justification for handling movement & collisions like this is that ive tried 2 other collision algorithms, didnt like 
     * them for what i was trying to achive, and decided that it was best that if i wanted to make a pixel precision type platformer
     * that i should just simply follow what celeste did to make theirs. one thing led to the next and i found documentation and 
     * a blog post from maddy (dev for celeste) explaining how the collision handling works */

    void Default() // default state settigns
    {
        if (abilityState == AbilityState.Default)
        {
            spriteScale = Vector2.Lerp(spriteScale, Vector2.one, 0.25f); // return sprite to normal

            // normal hitbox
            collisionBox.size = collisionBoxDefaultSize;
            collisionBox.offset = collisionBoxDefaultOffset;
        }
    }

    void CrouchAbility() // handles crouch movement ability
    {
        if (down && IsGrounded() && abilityState == AbilityState.Default) // start crouching
        {
            Crouch();
        }

        bool headRoom = !collideAt(ground, Vector2.up * 6 * pixel); // is there room above my head to uncrouch?
        if ((!down || !IsGrounded()) && abilityState == AbilityState.Crouch && headRoom) // stop crouching
        {
            abilityState = AbilityState.Default;
        }

        if (abilityState == AbilityState.Crouch) // crouching
        {
            spriteScale = Vector2.Lerp(spriteScale, Vector2.one, 0.25f); // return sprite to normal
            velocity += Mathf.Clamp(-velocity.x * inputX + crouchSpeed, 0, crouchSpeed * walkAcceleration) * inputX * Vector2.right; // capped crouch movement
        }
    }

    void WallSlide() // handles wall slide movement ability
    {
        if (IsSlidingAgainstWall()) // slide down walls
        {
            velocity.y = Mathf.Lerp(velocity.y, 0, 0.5f);
        } 
    }

    bool IsSlidingAgainstWall()
    {
        if (IsGrounded() || !IsTouchingWall())
        {
            return false;
        }

        if (velocity.y < 0 && inputX != 0) // slide down walls
        {
            return true;
        }

        return false;
    }

    void WallJump()
    {
        if (IsWallCoyoteGrounded() && !IsGrounded() && BufferedJumpInput()) // wall jump up
        {
            Debug.Log("Wall Jump");
            wallCoyoteCounter = 0;

            spriteScale = new Vector2(0.5f, 1.5f); // stretch
            velocity.y = jumpVelocity;
            int wallDirection = DirectionOfWall();

            if (wallDirection == 0)
            {
                velocity.x = walkSpeed * 1 * inputX;
            } 
            else
            {
                Debug.Log(-wallDirection);
                velocity.x = walkSpeed * 1 * -wallDirection;
                SetDirectionFromInt(-wallDirection);
            }
        }
    }

    int DirectionOfWall() // returns wether the contacting wall is to the left (-1) or to the right (1) or if there are two walls (0)
    {
        int direction = 0;
        bool left = collideAt(ground, Vector2.left);
        bool right = collideAt(ground, Vector2.right);

        if (left) { direction -= 1; }
        if (right) { direction += 1; }

        return direction;
    }

    void Crouch() // crouches the player // unused
    {
        abilityState = AbilityState.Crouch;

        spriteScale = new Vector2(1.5f, 0.5f); // squish

        // crouched hitbox
        collisionBox.size = collisionBoxCrouchedSize;
        collisionBox.offset = collisionBoxCrouchedOffset;
    }

    void Walk() // handles walking
    {
        if (IsGrounded() && abilityState == AbilityState.Default)
        {
            velocity += Mathf.Clamp(-velocity.x * inputX + walkSpeed, 0, walkSpeed * walkAcceleration) * inputX * Vector2.right; // capped acceleration
        }
    }

    void Jump() // handles jumping
    {
        if (IsCoyoteGrounded() && BufferedJumpInput())
        {
            Debug.Log("Jump");
            groundCoyoteCounter = 0; // reset coyote time

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
        // if ( no x input or x velocity is greater than walk speed ) && grounded
        if ((inputX == 0 || Mathf.Abs(velocity.x) > walkSpeed) && IsGrounded() && abilityState == AbilityState.Default)
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, 0.25f);
        }

        if ((inputX == 0 || Mathf.Abs(velocity.x) > crouchSpeed) && IsGrounded() && abilityState == AbilityState.Crouch)
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, 0.25f);
        }
    }

    void AirResistance() // handles air resistance
    {
        // player is trying to resist movement or isnt moving && they havent hit critical airspeed (no resistance)
        // if the input dir doesnt match the velocity dir && horizontal velocity is less than 2.2x the max airspeed
        if ((inputX * Mathf.Sign(velocity.x) <= 0) && (Mathf.Abs(velocity.x) < airSpeed * 2.2f) && !IsGrounded()) // this line is kinda hard to follow, hopefully the 2 explinations help
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, 0.05f);
        }

        // terminal velocity
        velocity.y = Mathf.Max(velocity.y, -2 * walkSpeed);
    }

    void Squish()
    {
/*        if (collideAt(ground, Vector2.zero) && state == State.Slide)
        {
            GameObject.Destroy(gameObject);
        }*/

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

    void SetDirectionFromInt(int directionInt)
    {
        //if(directionInt != 1 || directionInt != -1) { return;}

        if(directionInt == 1) { direction = FacingDirection.Right; }

        if (directionInt == -1) { direction = FacingDirection.Left; }
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
        jumping = Input.GetKey(KeyCode.Space); // jump

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
            variableJumpTimer = 4;
            return true;
        }
        return false;
    }

    void UpdateInput()
    {
        if (jump) { jumpBuffer = 10; } // half second
        if (jumpBuffer > 0) { jumpBuffer -= 1; }

        jump = false;
    }

    void UpdateCoyote()
    {
        if (IsGrounded())
        {
            groundCoyoteCounter = 5; // quarter second
        } 
        else if (groundCoyoteCounter > 0)
        {
            groundCoyoteCounter -= 1;
        }

    }

    void UpdateWallCoyote()
    {
        if (IsTouchingWall())
        {
            wallCoyoteCounter = 5; // quarter second
        }
        else if (wallCoyoteCounter > 0)
        {
            wallCoyoteCounter -= 1;
        }

    }

    void VariableJump() {
        if (variableJumpTimer > 0)
        {
            if (jumping)
                velocity.y = Mathf.Max(velocity.y, variableJumpSpeed);
            else
                variableJumpTimer = 0;
        }

        variableJumpTimer -= 1;
    }

    void Gravity()
    {
        if (!IsGrounded())
        {
            float halfGravityThreshold = 2;
            float multiplier = (Mathf.Abs(velocity.y) < halfGravityThreshold && jumping) ? 0.5f : 1f;


            //velocity.y = Mathf.Lerp(velocity.y, -12, gravity * multiplier);

            velocity.y -= gravity * multiplier;
            velocity.y = Mathf.Max(velocity.y, -12);
        }
    }

    public void SetInheritedVelocity(Vector2 inheritedVelocity)
    {
        this.inheritedVelocity = inheritedVelocity;
    }

    void UpdateGraphics()
    {
        graphicsState = GraphicsState.Idle;

        if (inputX != 0) { graphicsState = GraphicsState.Walking; }
        
        if (!IsGrounded()) { graphicsState = GraphicsState.Jump; }

        if (IsSlidingAgainstWall()) { graphicsState = GraphicsState.WallSlide; }

        if (abilityState == AbilityState.Crouch) { graphicsState = GraphicsState.Crouch; }

        if (abilityState == AbilityState.Crouch && inputX != 0) { graphicsState = GraphicsState.CrouchWalking; }
    }
}
