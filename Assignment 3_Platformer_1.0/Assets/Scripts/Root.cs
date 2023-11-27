using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeScreen(BoxCollider2D screen)
    {
        BroadcastMessage("ChangeDollyTrackedScreen", screen);
    }
}
