using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
   public void ReplayGame() 
   {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
   }

   public void ExitGame()
   {
        Application.Quit();
   }
}
