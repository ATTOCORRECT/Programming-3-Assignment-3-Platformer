using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulledString : MonoBehaviour
{ 
    Vector3 y, d;

    public float r;
    public Transform targetTransform;

    void Start()
    {
        y = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 x = targetTransform.position;

        d = x - y; // displacement vector from y to x

        d = Vector3.ClampMagnitude(d, r);

        y = x - d;

        transform.position = y;
    }

    Vector3 getDynamicVector()
    {
        return y;
    }
}
