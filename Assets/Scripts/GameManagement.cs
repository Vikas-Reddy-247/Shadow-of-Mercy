using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManagement : MonoBehaviour
{
    public bool scathed;
    public bool detected;

    public TextMeshProUGUI unscathedText = null;
    public TextMeshProUGUI scathedText = null;
    public TextMeshProUGUI undetectedText = null;
    public TextMeshProUGUI detectedText = null;
    public TextMeshProUGUI promoteNovNinja = null;
    public TextMeshProUGUI promoteExpNinja = null;
    public TextMeshProUGUI promoteMasNinja = null;
    public Canvas gameOverCanvas;

    void Start()
    {
        unscathedText.gameObject.SetActive(false);
        scathedText.gameObject.SetActive(false);
        undetectedText.gameObject.SetActive(false);
        detectedText.gameObject.SetActive(false);
        promoteNovNinja.gameObject.SetActive(false);
        promoteExpNinja.gameObject.SetActive(false);
        promoteMasNinja.gameObject.SetActive(false);
        gameOverCanvas.enabled = false;
        scathed = false;
        detected = false;
    }

    void Update()
    {
        if(scathed)
        {
            scathedText.gameObject.SetActive(true);
        }

        if(detected)
        {
            detectedText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(detected && scathed)
            {
                promoteNovNinja.gameObject.SetActive(true);
            }

            if(detected && !scathed)
            {
                promoteExpNinja.gameObject.SetActive(true);
                unscathedText.gameObject.SetActive(true);

            }

            if(!detected && !scathed)
            {
                promoteMasNinja.gameObject.SetActive(true);
                undetectedText.gameObject.SetActive(true);
                unscathedText.gameObject.SetActive(true);
            }

            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            gameOverCanvas.enabled = true;
        }
    }
}
