using UnityEngine;

public static class PlayerInput
{
    public const string AXIS_HORIZONTAL = "Horizontal";
    public const string AXIS_VERTICAL = "Vertical";

    public const string BUTTON_JUMP = "Jump";

    /// <summary>
    /// Provides the current input from directional controls (arrow keys, d-pad, left joystick).
    /// </summary>
    /// <returns>A raw Vector2 with horizontal input stored in the x axis and vertical input stored int eh y axis.</returns>
    public static Vector2 GetDirectionalInput()
    {
        return new Vector2(Input.GetAxisRaw(AXIS_HORIZONTAL), Input.GetAxisRaw(AXIS_VERTICAL));
    }

    /// <summary>
    /// Returns whether the jump button was pressed on this frame.
    /// </summary>
    /// <returns>True the frame the jump button is pressed, false otherwise.</returns>
    public static bool WasJumpPressed()
    {
        return Input.GetButtonDown(BUTTON_JUMP);
    }

    /// <summary>
    /// Returns whether the jump button is being help on this frame.
    /// </summary>
    /// <returns>True is the jump button is being pressed, false is released.</returns>
    public static bool IsJumpPressed()
    {
        return Input.GetButton(BUTTON_JUMP);
    }
}
