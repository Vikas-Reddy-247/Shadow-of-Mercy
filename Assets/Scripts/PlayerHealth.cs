using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Image healthDisplay;
    public float regenerationDelay = 10f;
    public float regenerationRate = 5f;
    public bool initiateDamage;
    public bool isDowned = false;
    public Image damageScreen = null;
    Animator anim;
    GuardAI guardAI;
    GameManagement gameManagement;
    private Coroutine regenerationCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        GameObject manage = GameObject.FindGameObjectWithTag("Relic");
        gameManagement = manage.GetComponent<GameManagement>();
        GameObject[] guards = GameObject.FindGameObjectsWithTag("Guard");
        foreach (GameObject guard in guards)
        {
            guardAI = guard.GetComponent<GuardAI>();
        }
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        HealthDisplayFiller();

        //If initiate damage is called by arrow script the player takes 25 damage
        if (initiateDamage)
        {
            TakeDamage(25);
            initiateDamage = false;
        }

        //The damage VFX screen tranparency is changed based on player's health
        switch(currentHealth)
        {
            case 100: damageScreen.color = new Color (damageScreen.color.r, damageScreen.color.g, damageScreen.color.b, 0f); break;
            case 75: damageScreen.color = new Color (damageScreen.color.r, damageScreen.color.g, damageScreen.color.b, 0.25f); break;
            case 50: damageScreen.color = new Color (damageScreen.color.r, damageScreen.color.g, damageScreen.color.b, 0.5f); break;
            case 25: damageScreen.color = new Color (damageScreen.color.r, damageScreen.color.g, damageScreen.color.b, 0.75f); break;
            case 0: damageScreen.color = new Color (damageScreen.color.r, damageScreen.color.g, damageScreen.color.b, 1f); break;
        }

        //If health is less than 100 regeneration coroutine starts
        if(currentHealth < 100)
        {
            regenerationCoroutine = StartCoroutine(RegenerateHealth());
            //Checks the optional objective of Unscathed as player was hurt
            gameManagement.scathed = true;
        }
    }

    void HealthDisplayFiller()
    {
        float healthNormalized = (float)currentHealth / maxHealth;
        healthDisplay.fillAmount = healthNormalized;
    }

    //Health reduction process is done and Unconscious bool is called by other scripts if health is less than zero
    void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Unconscious();
        }
    }

    //If arrow collides with player when guard fires initiate damage is done
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 7)
        {
            initiateDamage = true;
        }
    }

    //If player is less than zero then player defeat process is done
    void Unconscious()
    {
        anim.SetTrigger("playerdowned");
        this.enabled = false;
        isDowned = true;
        guardAI.HandlePlayerKnocked();
    }

    //This method is called when animation of player down finished and UI is handled
    public void HandleKO()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameManagement.gameOverCanvas.enabled = true;
    }

    //Regenration of health overtime is done after few seconds when this coroutine starts
    IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(regenerationDelay);

        float elapsedTime = 0f;

        while (currentHealth < maxHealth)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / regenerationDelay;
            int amountToRegenerate = Mathf.RoundToInt(Mathf.Lerp(0, maxHealth - currentHealth, percentageComplete));
            currentHealth += amountToRegenerate;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            yield return null;
        }
    }
}
