using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObjects : MonoBehaviour
{
    //Detroys on collision by specified name tag.
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Disc"))
        {
            Destroy(gameObject);
        }
    }
}
