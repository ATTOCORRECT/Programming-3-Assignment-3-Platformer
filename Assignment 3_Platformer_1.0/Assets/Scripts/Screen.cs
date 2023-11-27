using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screen : MonoBehaviour
{
    public BoxCollider2D bounds, trigger;
    // Start is called before the first frame update
    void Start()
    {
        trigger.size = bounds.size - new Vector2(32,32);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                SendMessageUpwards("ChangeScreen", bounds);
            }
        }
    }
}
