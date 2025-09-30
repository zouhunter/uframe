using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCtrol : MonoBehaviour
{
    // Start is called before the first frame update
    public void Start()
    {
        unsafe
        {
            int b = 2;
            int* a = &b;
            Debug.LogError(*a);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
