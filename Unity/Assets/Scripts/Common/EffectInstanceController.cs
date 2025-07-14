using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectInstanceController : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 5f; // Default lifetime of the effect instance


    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime); // Destroy the effect instance after its lifetime    
    }
}
