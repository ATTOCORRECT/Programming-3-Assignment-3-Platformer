using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dolly : MonoBehaviour
{

    Vector2 targetPosition, position;

    private void Start()
    {
        targetPosition = transform.position;
        position = targetPosition;
    }

    void FixedUpdate()
    {
        transform.position = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y)); // pixel perfect!

        position = Vector2.Lerp(position, targetPosition, 0.2f);
    }

    void ChangeDollyTrackedScreen(BoxCollider2D Screen)
    {
        targetPosition = Screen.transform.position;
    }
}
