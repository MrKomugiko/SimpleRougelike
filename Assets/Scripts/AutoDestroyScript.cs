using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyScript : MonoBehaviour
{
    public float Time = 0.5f;
    void Start()
    {
        Destroy(this.gameObject,Time);
    }
}
