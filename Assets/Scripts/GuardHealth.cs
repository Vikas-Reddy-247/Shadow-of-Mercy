using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GuardHealth : MonoBehaviour
{
    public int maxHp = 100;
    public int guardHp;
    public bool initiateDamage;
    public bool isUnconscious = false;
    public bool isAlert = false;
    public bool iselectrocuted = false;
    public Slider healthSlider = null;
    TechDisc techDisc;

    void Start()
    {
        guardHp = maxHp;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        techDisc = player.GetComponentInChildren<TechDisc>();
    }

    void Update()
    {
        //If initiate damage is called by TechDisc script the guard takes 50 damage and updates in the UI
        if (initiateDamage)
        {
            TakeDamage(50);
            isAlert = true;
            initiateDamage = false;
        }
        healthSlider.value = guardHp;
    }

    //Health reduction process is done and  isUnconscious bool is called by other scripts if health is less than zero
    public void TakeDamage(int damage)
    {
        guardHp -= damage;

        if (guardHp <= 0)
        {
            isUnconscious = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //If disc collides with guard when thrown calls initiate damage
        if (collision.gameObject.CompareTag("Disc"))
        {
            initiateDamage = true;
        }

         //If disc collides with guard when shock ability is active then guard electrocuted mechanic is done
        if(techDisc.shockActive)
        {
            if (collision.gameObject.CompareTag("Disc"))
            {
                iselectrocuted = true;
                techDisc.shockActive = false;
                techDisc.ReduceCurrentCharge();
            }
        }
    }
}
