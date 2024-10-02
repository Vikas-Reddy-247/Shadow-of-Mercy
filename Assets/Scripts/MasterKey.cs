using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterKey : MonoBehaviour
{
    MasterVault masterVault;
    void Start()
    {
        GameObject vault = GameObject.FindGameObjectWithTag("Door");
        masterVault = vault.GetComponent<MasterVault>();
    }

    private void OnTriggerEnter(Collider other)
    {
       if (other.gameObject.CompareTag("Player"))
       {
            masterVault.hasKey = true;
            Destroy(gameObject);
       }
    }
}
