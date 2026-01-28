using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("MainLayout"); 
    }
    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }
    public void QuitGame()
    {
        
        Application.Quit();       
    }
}
