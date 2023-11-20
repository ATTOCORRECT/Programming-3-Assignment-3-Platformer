using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform endPositionTransform;
    public BoxCollider2D collisionBox;
    Vector2 startPosition, endPosition, position, lastPosition, velocity, lastVelocity, acceleration;
    void Start()
    {
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

        lastPosition = position;
        lastVelocity = velocity;

        float t = Mathf.Sin(Time.fixedTime)*0.55f + 0.5f;
        position = Vector2.Lerp(startPosition, endPosition, t);

        velocity = position - lastPosition;
        acceleration = velocity - lastVelocity;

        CollideV();
        CollideH();
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

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, LayerMask.GetMask("Player"));
            Debug.DrawRay(rayOrigin, rayDirection.normalized * rayDistance, Color.red);

            if (hit)
            {
                Debug.Log("HIT");
                Vector2 push = new Vector2((Mathf.Abs(velocity.x) - (hit.distance - offset)) * Mathf.Sign(velocity.x), 0);
                hit.collider.gameObject.GetComponentInParent<PlayerController>().Push(push);
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

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, LayerMask.GetMask("Player"));
            Debug.DrawRay(rayOrigin, rayDirection.normalized * rayDistance, Color.red);

            if (hit)
            {
                Vector2 push = new Vector2(0, (Mathf.Abs(velocity.y) - (hit.distance - offset)) * Mathf.Sign(velocity.y));
                hit.collider.gameObject.GetComponentInParent<PlayerController>().Push(push);
                return;
            }
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
