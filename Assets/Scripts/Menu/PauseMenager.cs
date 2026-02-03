using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Added for Slider
using System.Linq; // Added for FindSliderByName

public class PauseMenu : MonoBehaviour
{
    [Header("Graphics Settings Sliders")]
    public Slider gammaSlider;
    public Slider contrastSlider;

    [Header("Main Panels")]
    public GameObject pauseMenuUI;      // Główne menu (Play, Settings, Info, Quit)
    public GameObject settingsPanelUI;  // Menu ustawień (Mouse, Audio, Graphics...)
    public GameObject infoPanelUI;      // Menu info (Lore, About Team)

    [Header("Info Sub-Panels")]
    public GameObject lorePanelUI;      // Panel z historią (Lore)
    public GameObject aboutTeamPanelUI; // Panel o twórcach (About Team)

    [Header("Settings Sub-Panels")]
    public GameObject mouseSettingsUI;
    public GameObject soundSettingsUI;
    public GameObject graphicsSettingsUI;

    [Header("Keyboard Pages")]
    public GameObject keyboardPage1;
    public GameObject keyboardPage2;

    private bool isPaused = false;

    void Start()
    {
        // Auto-fix (dla bezpieczeństwa)
        if (pauseMenuUI == null)
        {
            var panel = transform.Find("PausePanel");
            if (panel != null) pauseMenuUI = panel.gameObject;
        }

        CloseAllMenus(); // Na start wszystko wyłączone

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // --- Auto-Wire Sliders ---
        if (gammaSlider == null)
        {
            gammaSlider = FindSliderByName("Gamma");
            if (gammaSlider == null) gammaSlider = FindSliderByName("Jasność");
            if (gammaSlider == null) gammaSlider = FindSliderByName("Jasnosc");
            if (gammaSlider == null) gammaSlider = FindSliderByName("Brightness");
        }
        if (contrastSlider == null)
        {
            contrastSlider = FindSliderByName("Contrast");
            if (contrastSlider == null) contrastSlider = FindSliderByName("Kontrast");
        }

        // Apply Limits
        if (gammaSlider != null) { gammaSlider.minValue = -0.5f; gammaSlider.maxValue = 0.5f; }
        if (contrastSlider != null) { contrastSlider.minValue = -99f; contrastSlider.maxValue = 99f; }

        if (gammaSlider != null)
        {
            gammaSlider.onValueChanged.RemoveAllListeners();
            gammaSlider.onValueChanged.AddListener(SetGamma);
        }
        if (contrastSlider != null)
        {
            contrastSlider.onValueChanged.RemoveAllListeners();
            contrastSlider.onValueChanged.AddListener(SetContrast);
        }
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Logika cofania przyciskiem ESCAPE

            // 1. Jeśli jesteśmy głęboko w ustawieniach (np. Mysz), wróć do Settings
            if (AreSubSettingsOpen())
            {
                BackToSettings();
                return;
            }

            // 2. Jeśli jesteśmy w Lore/Team, wróć do Info
            if (lorePanelUI != null && lorePanelUI.activeSelf ||
                aboutTeamPanelUI != null && aboutTeamPanelUI.activeSelf)
            {
                BackToInfo();
                return;
            }

            // 3. Jeśli jesteśmy w Settings lub Info, wróć do głównej Pauzy
            if ((settingsPanelUI != null && settingsPanelUI.activeSelf) ||
                (infoPanelUI != null && infoPanelUI.activeSelf))
            {
                BackToPauseMenu();
                return;
            }

            // 4. Jeśli jesteśmy w głównej pauzie -> wznów grę, jeśli w grze -> pauzuj
            if (isPaused) Resume();
            else Pause();
        }
    }

    // --- GŁÓWNE FUNKCJE STANU GRY ---
    public void Resume()
    {
        CloseAllMenus();
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Pause()
    {
        CloseAllMenus();
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        AudioListener.pause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // --- FUNKCJE POWROTU (BACK BUTTONS) ---

    // Podepnij to pod przycisk "Back" w panelu SETTINGS oraz INFO
    public void BackToPauseMenu()
    {
        CloseAllMenus();
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
    }

    // Podepnij to pod przycisk "Back" w panelach MOUSE, SOUND, GRAPHICS, KEYBOARD
    public void BackToSettings()
    {
        CloseAllMenus();
        if (settingsPanelUI != null) settingsPanelUI.SetActive(true);
    }

    // Podepnij to pod przycisk "Back" w panelach LORE i ABOUT TEAM
    public void BackToInfo()
    {
        CloseAllMenus();
        if (infoPanelUI != null) infoPanelUI.SetActive(true);
    }


    // --- NAWIGACJA DO PRZODU ---

    public void OpenSettings()
    {
        CloseAllMenus();
        if (settingsPanelUI != null) settingsPanelUI.SetActive(true);
    }

    public void OpenInfo()
    {
        CloseAllMenus();
        if (infoPanelUI != null) infoPanelUI.SetActive(true);
    }

    public void OpenLore()
    {
        CloseAllMenus();
        if (lorePanelUI != null) lorePanelUI.SetActive(true);
    }

    public void OpenAboutTeam()
    {
        CloseAllMenus();
        if (aboutTeamPanelUI != null) aboutTeamPanelUI.SetActive(true);
    }

    // --- SUB-SETTINGS ---
    public void OpenMouseSettings()
    {
        CloseAllMenus();
        if (mouseSettingsUI != null) mouseSettingsUI.SetActive(true);
    }

    public void OpenSoundSettings()
    {
        CloseAllMenus();
        if (soundSettingsUI != null) soundSettingsUI.SetActive(true);
    }

    public void OpenGraphicsSettings()
    {
        CloseAllMenus();
        if (graphicsSettingsUI != null) graphicsSettingsUI.SetActive(true);
    }

    // --- OBSŁUGA KLAWIATURY (2 STRONY) ---
    public void OpenKeyboardSettings()
    {
        OpenKeyboardPage1();
    }

    public void OpenKeyboardPage1()
    {
        CloseAllMenus();
        if (keyboardPage1 != null) keyboardPage1.SetActive(true);
    }

    public void OpenKeyboardPage2()
    {
        CloseAllMenus();
        if (keyboardPage2 != null) keyboardPage2.SetActive(true);
    }

    // --- POMOCNICZE ---
    void CloseAllMenus()
    {
        // Główne
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsPanelUI != null) settingsPanelUI.SetActive(false);
        if (infoPanelUI != null) infoPanelUI.SetActive(false);

        // Info sub-panele
        if (lorePanelUI != null) lorePanelUI.SetActive(false);
        if (aboutTeamPanelUI != null) aboutTeamPanelUI.SetActive(false);

        // Settings sub-panele
        if (mouseSettingsUI != null) mouseSettingsUI.SetActive(false);
        if (soundSettingsUI != null) soundSettingsUI.SetActive(false);
        if (graphicsSettingsUI != null) graphicsSettingsUI.SetActive(false);
        if (keyboardPage1 != null) keyboardPage1.SetActive(false);
        if (keyboardPage2 != null) keyboardPage2.SetActive(false);
    }

    bool AreSubSettingsOpen()
    {
        return (mouseSettingsUI != null && mouseSettingsUI.activeSelf) ||
               (soundSettingsUI != null && soundSettingsUI.activeSelf) ||
               (graphicsSettingsUI != null && graphicsSettingsUI.activeSelf) ||
               (keyboardPage1 != null && keyboardPage1.activeSelf) ||
               (keyboardPage2 != null && keyboardPage2.activeSelf);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // --- GRAPHICS CONTROL ---
    public void SetGamma(float val)
    {
        EnsureGammaController();
        if (GammaController.Instance != null)
        {
            GammaController.Instance.SetGamma(val);
        }
    }

    public void SetContrast(float val)
    {
        EnsureGammaController();
        if (GammaController.Instance != null)
        {
            GammaController.Instance.SetContrast(val);
        }
    }

    private void EnsureGammaController()
    {
        if (GammaController.Instance == null)
        {
            var gc = FindFirstObjectByType<GammaController>();
            if (gc == null)
            {
                GameObject go = new GameObject("GammaController_Auto");
                gc = go.AddComponent<GammaController>();
            }
            // GammaController.Awake sets Instance
        }
    }

    private Slider FindSliderByName(string partialName)
    {
        // Search in inactive components too, since settings panel might be closed
        return GetComponentsInChildren<Slider>(true)
            .FirstOrDefault(s => s.name.IndexOf(partialName, System.StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
