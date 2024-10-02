using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TechDisc : MonoBehaviour
{
    public bool activated;
    public float rotationSpeed;
    public bool collidedToGuard;
    public bool shockActive = false;
    public bool iselectrocuted = false;
    public int chargeAmount = 0;
    public ParticleSystem electric;
    public ParticleSystem distract;
    public Image ability;
    public TextMeshProUGUI abilityText;
    GuardAI guardAI;

    void Start()
    {
        GameObject guard = GameObject.FindGameObjectWithTag("Guard");
        guardAI = guard.GetComponent<GuardAI>();
    }
    void Update()
    {
        //When disc is thrown the disc rotates clockwise horizontally
        if (activated)
        {
            transform.localEulerAngles += Vector3.up * rotationSpeed * Time.deltaTime;
        }

        //When key is pressed handles the UI fade, Particale System and Shock ability
        if(chargeAmount > 0)
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                if(!shockActive)
                {
                    electric.Play();
                    shockActive = true;
                    ability.color = new Color (ability.color.r, ability.color.g, ability.color.b, 0.5f);
                }
                else
                {
                    electric.Stop();
                    shockActive = false;
                    ability.color = new Color (ability.color.r, ability.color.g, ability.color.b, 1f);
                }
            }
        }

        //Displays no. of shock charges
        abilityText.text = chargeAmount.ToString();

        if(!shockActive)
        {
            electric.Stop();
        }

        //Handles distract particle system
        if(Input.GetKeyDown(KeyCode.E))
        {
            distract.Play();
        }
    }

    public int currentCharge()
    {
        return chargeAmount;
    }

    public void ReduceCurrentCharge()
    {
        chargeAmount --;
    }

    //Handle disc freeze in that position when collided with surfaces or guards
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 0 || collision.gameObject.layer == 9 || collision.gameObject.layer == 6)
        {
            GetComponent<Rigidbody>().Sleep();
            GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            GetComponent<Rigidbody>().isKinematic = true;
            activated = false;
        }

        if (collision.gameObject.layer == 6 && !shockActive)
        {
            collidedToGuard = true;
        }
    }
}
