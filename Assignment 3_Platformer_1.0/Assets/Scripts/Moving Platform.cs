using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovingPlatform : MonoBehaviour
{
    public Transform endPositionTransform;

    public BoxCollider2D collisionBox;

    public float Speed = 0.5f;
    public float frequency = 1;

    Vector2 startPosition, endPosition, position, currentPosition, lastPosition, velocity, acceleration, remainder;

    GameObject player;
    PlayerController playerController;
    BoxCollider2D playerCollisionBox;
    LayerMask playerlayer;

    float cycle = 0;

    void Start()
    {
        startPosition = transform.position;
        endPosition = endPositionTransform.position;
        position = startPosition;
        currentPosition = startPosition;
        lastPosition = startPosition;
        velocity = Vector2.zero;
        acceleration = Vector2.zero;

        player = GameObject.Find("Buddy");
        playerController = player.GetComponent<PlayerController>();
        playerCollisionBox = player.GetComponentInChildren<BoxCollider2D>();
        playerlayer = LayerMask.GetMask("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lastPosition = currentPosition;

        float t = Mathf.Clamp01(Mathf.Sin(cycle * 2 * Mathf.PI) * Speed + 0.5f);
        currentPosition = Vector2.Lerp(startPosition, endPosition, t);

        velocity = currentPosition - lastPosition;

        if (IsBeingRidden()) // give player platform velocity
        {
            playerController.SetInheritedVelocity(velocity);
        }

        Move(velocity.x, velocity.y);

        cycle += 0.05f * frequency;
        cycle %= 1;
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

    public void Move(float x, float y)
    {
        remainder.x += x;
        remainder.y += y;
        int moveX = Mathf.RoundToInt(remainder.x);
        int moveY = Mathf.RoundToInt(remainder.y);

        if (moveX != 0 || moveY != 0)
        {
            bool isBeingRidden = IsBeingRidden();
            collisionBox.enabled = false;

            if (moveX != 0)
            {
                remainder.x -= moveX;
                position.x += moveX;

                int signX = (int)Mathf.Sign(moveX);

                if (collideAt(playerlayer, new Vector2(-moveX,0)))
                {
                    // Push
                    int platformEdgeX = (int)(position.x + collisionBox.size.x / 2 * signX + collisionBox.offset.x); // the left or right edge of the platform
                    int playerEdgeX = (int)(playerController.position.x - playerCollisionBox.size.x / 2 * signX + playerCollisionBox.offset.x); // the left or right edge of the player
                    int pushX = platformEdgeX - playerEdgeX;
                    playerController.MoveX(pushX);
                }
                else if (isBeingRidden)
                {
                    // Carry
                    playerController.MoveX(moveX);
                }
            }

            if (moveY != 0)
            {
                remainder.y -= moveY;
                position.y += moveY;

                int signY = (int)Mathf.Sign(moveY);

                if (collideAt(playerlayer, new Vector2(0, -moveY)))
                {
                    // Push
                    int platformEdgeY = (int)(position.y + collisionBox.size.y / 2 * signY + collisionBox.offset.y); // the top or bottom edge of the platform
                    int playerEdgeY = (int)(playerController.position.y - playerCollisionBox.size.y / 2 * signY + playerCollisionBox.offset.y); // the top or bottom edge of the player
                    int pushY = (platformEdgeY - playerEdgeY);
                    playerController.MoveY(pushY);
                }
                else if (isBeingRidden)
                {
                    // Carry
                    playerController.MoveY(moveY);
                }
            }

            collisionBox.enabled = true;
        }
        transform.position = position;
    }

    bool IsBeingRidden()
    {
        if (GameObject.Find("Buddy") != null)
        {
            return playerController.IsRiding(gameObject);
        }
        else
        {
            return false;
        }
    }

    public Vector2 getAcceleration()
    {
        return acceleration;
    }
    public Vector2 getVelocity()
    {
        return velocity;
    }
}
