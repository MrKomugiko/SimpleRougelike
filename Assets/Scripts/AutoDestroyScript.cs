using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyScript : MonoBehaviour
{
    void Start()
    {
        Destroy(this.gameObject,.5f);
    }
}
