using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardBody : MonoBehaviour
{
    public float radius = 1.4f;
    Transform player;
    GuardAI guardAI;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        guardAI = GetComponent<GuardAI>();
    }

    void Update()
    {
        //Calculates the distance btw guard and player is less than specified radius
        float disToPlayer = Vector3.Distance(transform.position, player.position);
        if(disToPlayer <= radius)
        {
            //If guard is Unconscious and pressed key guard object set active to false
            if(Input.GetKeyDown(KeyCode.R) && !guardAI.isConscious)
            {
                Camouflage();
            }
        }
    }

    void Camouflage()
    {
        gameObject.SetActive(false);
    }
}
