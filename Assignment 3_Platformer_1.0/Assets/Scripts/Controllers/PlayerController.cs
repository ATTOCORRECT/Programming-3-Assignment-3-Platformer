using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection { Left, Right }
    public float topSpeed;
    public FacingDirection direction;
    bool up, left, down, right, jump, crouch, special;

    public bool IsWalking()
    {
        return false;
    }

    public bool IsGrounded()
    {
        bool grounded = Physics2D.Raycast(transform.position, Vector2.down * (1f/16f), 1f, LayerMask.GetMask("Ground"));
        Debug.DrawRay(transform.position, Vector2.down * ((2f / 16f) + 0.5f));
        return grounded;
    }

    public FacingDirection GetFacingDirection()
    {
        return direction;
    }

    private void Update()
    {
        Debug.Log(IsGrounded());

        PlayerInput();
        UpdateDirection();
        PlayerMovement();
    }

    void PlayerMovement()
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
