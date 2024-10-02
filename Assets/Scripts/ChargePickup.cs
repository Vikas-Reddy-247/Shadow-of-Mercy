using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargePickup : MonoBehaviour
{
    TechDisc techDisc;

    void Start()
    {
        //Access another script from different object.

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        techDisc = player.GetComponentInChildren<TechDisc>();
    }

    //Add charge and destroy on collision
    private void OnTriggerEnter(Collider other)
    {
       if (other.gameObject.CompareTag("Player"))
       {
            techDisc.chargeAmount ++;
            Destroy(gameObject);
       }
    }
}
