using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterVault : MonoBehaviour
{
    public bool hasKey = false;
    public bool canOpen;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F) && hasKey && canOpen)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
           canOpen = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
         if (other.gameObject.tag == "Player")
        {
           canOpen = false;
        }
    }
}
