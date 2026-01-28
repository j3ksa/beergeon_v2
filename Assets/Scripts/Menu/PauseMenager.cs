using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        AudioListener.pause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void Settings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void QuitGame()
    {
        
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}