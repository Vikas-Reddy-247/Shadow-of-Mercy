using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapBox : MonoBehaviour
{
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    //Handles the trap object's position and falling
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Disc"))
        {
            rb.isKinematic = false;
        }

        if (other.CompareTag("Environment"))
        {
            rb.isKinematic = true;
        }
    }
}
