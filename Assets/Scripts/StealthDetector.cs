using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class StealthDetector : MonoBehaviour
{
    GuardAI guardAI;
    public bool canBeTakenDown;
    PlayerGeneral playerGen;

    void Start()
    {
        guardAI = transform.parent.GetComponent<GuardAI>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerGen = player.GetComponent<PlayerGeneral>();
    }

    void Update()
    {
        //When guard is Conscious and if key is pressed then animation and method of other scripts for this mechanic executes
        if(Input.GetKeyDown(KeyCode.F) && guardAI.isConscious && canBeTakenDown)
        {
            playerGen.StealthTakedown();
            guardAI.takedownButton.SetActive(false);
            guardAI.GetTakendown();
        }

        if(!guardAI.isConscious)
        {
            guardAI.takedownButton.SetActive(false);
        }
    }

    //UI button appear if the player is behind guard
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            guardAI.takedownButton.SetActive(true);
            canBeTakenDown = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
       if (other.gameObject.tag == "Player")
        {
            guardAI.takedownButton.SetActive(false);
        } 
    }
}
