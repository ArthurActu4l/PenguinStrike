using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestruct : MonoBehaviour
{
    public float time = 5f;
    void Update()
    {
        Destroy(gameObject, time);
    }
}
