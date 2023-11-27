using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform endPositionTransform;
    public BoxCollider2D collisionBox;
    Vector2 startPosition, endPosition, position, currentPosition, lastPosition, velocity, lastVelocity, acceleration, remainder;
    GameObject player;
    PlayerController playerController;
    BoxCollider2D playerCollisionBox;
    LayerMask playerlayer;

    void Start()
    {
        player = GameObject.Find("Buddy");
        playerController = player.GetComponent<PlayerController>();
        playerCollisionBox = player.GetComponentInChildren<BoxCollider2D>();
        playerlayer = LayerMask.GetMask("Player");
        startPosition = transform.position;
        endPosition = endPositionTransform.position;
        position = startPosition;
        currentPosition = startPosition;
        lastPosition = startPosition;
        velocity = Vector2.zero;
        lastVelocity = Vector2.zero;
        acceleration = Vector2.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        lastPosition = currentPosition;

        float t = Mathf.Sin(Time.fixedTime)*0.55f + 0.5f;
        currentPosition = Vector2.Lerp(startPosition, endPosition, t);

        velocity = currentPosition - lastPosition;

        Move(velocity.x, velocity.y);
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
            bool isBeingRidden = playerController.IsRiding(gameObject);
            collisionBox.enabled = false;

            if (moveX != 0)
            {
                remainder.x -= moveX;
                position.x += moveX;

                int signX = (int)Mathf.Sign(moveX);

                if (collideAt(playerlayer, new Vector2(-moveX,0)))
                {
                    // Push
                    int pushX = (int)((position.x + collisionBox.size.x / 2 * signX) - (playerController.position.x - playerCollisionBox.size.x / 2 * signX));
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
                    int pushY = (int)((position.y + collisionBox.size.y / 2 * signY) - (playerController.position.y - playerCollisionBox.size.y / 2 * signY + playerCollisionBox.offset.y));
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
        // HERE https://www.maddymakesgames.com/articles/celeste_and_towerfall_physics/index.html
        transform.position = position;
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
