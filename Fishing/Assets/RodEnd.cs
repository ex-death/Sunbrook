using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodEnd : MonoBehaviour
{
    public Transform rodEnd;
    
    void FixedUpdate()
    {
        transform.position = rodEnd.position;
    }
}
