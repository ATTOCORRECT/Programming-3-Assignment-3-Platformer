using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screen : MonoBehaviour
{
    public BoxCollider2D collider;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enter!");
    }

    /*    private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log("ENTER!");
            if (collision != null)
            {
                if (collision.gameObject.layer == LayerMask.GetMask("Player"))
                {
                    SendMessageUpwards("ChangeScreen", collider);
                }
            }
        }*/
}
