using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform endPositionTransform;
    public BoxCollider2D collisionBox;
    Vector2 startPosition, endPosition, position, lastPosition, velocity, lastVelocity, acceleration, remainder;
    GameObject player;
    void Start()
    {
        player = GameObject.Find("Buddy");
        startPosition = transform.position;
        endPosition = endPositionTransform.position;
        position = startPosition;
        lastPosition = startPosition;
        velocity = Vector2.zero;
        lastVelocity = Vector2.zero;
        acceleration = Vector2.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = position;

        float t = Mathf.Sin(Time.fixedTime)*0.55f + 0.5f;
        velocity = Vector2.Lerp(startPosition, endPosition, t);

        Move(velocity.x, velocity.y);

        Debug.Log(player.GetComponent<PlayerController>().IsRiding(gameObject));
    }

    public void Move(float x, float y)
    {
        remainder.x += x;
        remainder.y += y;
        int moveX = Mathf.RoundToInt(remainder.x);
        int moveY = Mathf.RoundToInt(remainder.y);

        if (moveX != 0 || moveY != 0)
        {
            if (moveX != 0)
            {
                remainder.x -= moveX;
                position.x += moveX;
            }
        }
        // HERE https://www.maddymakesgames.com/articles/celeste_and_towerfall_physics/index.html

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
