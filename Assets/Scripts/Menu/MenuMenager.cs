using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // For Button types if needed
using UnityEngine.InputSystem;
using System.Linq;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuUI;
    public GameObject settingsPanelUI;

    [Header("Animation")]
    public settingsAnimation settingsAnim;

    [Header("Settings Sub-Panels")]
    public GameObject mousePanelUI;
    public settingsAnimation mouseAnim;

    public GameObject soundPanelUI;
    public settingsAnimation soundAnim;

    public GameObject graphicsPanelUI;
    public settingsAnimation graphicsAnim;

    public GameObject keyVar1PanelUI;
    public settingsAnimation keyVar1Anim;

    public GameObject keyVar2PanelUI;
    public settingsAnimation keyVar2Anim;

    [Header("Info Panels")]
    public GameObject infoPanelUI;
    public settingsAnimation infoAnim;

    public GameObject lorePanelUI;
    public settingsAnimation loreAnim;

    public GameObject aboutTeamPanelUI;
    public settingsAnimation teamAnim;

    void Start()
    {
        // --- Auto-Find Panels ---
        if (mainMenuUI == null) mainMenuUI = FindPanel("MainPanel", "MainMenu");
        if (settingsPanelUI == null) settingsPanelUI = FindPanel("SettingsPanel", "SettingsMenu", "Settings"); // Added "Settings"

        // Info System Auto-Find
        if (infoPanelUI == null) infoPanelUI = FindPanel("Info", "InfoPanel");
        if (lorePanelUI == null) lorePanelUI = FindPanel("Lore", "LorePanel");
        if (aboutTeamPanelUI == null) aboutTeamPanelUI = FindPanel("AboutTeam", "TeamPanel");

        // Settings Sub-Panels Auto-Find
        if (mousePanelUI == null) mousePanelUI = FindPanel("MouseSettings", "Mouse");
        if (soundPanelUI == null) soundPanelUI = FindPanel("Sound", "SoundSettings");
        if (graphicsPanelUI == null) graphicsPanelUI = FindPanel("GraphicsSettings", "Graphics");
        if (keyVar1PanelUI == null) keyVar1PanelUI = FindPanel("KeySettingsVar1", "Keyboard1");
        if (keyVar2PanelUI == null) keyVar2PanelUI = FindPanel("KeySettingsVar2", "Keyboard2");

        // --- Auto-Find Animations ---
        if (settingsAnim == null) settingsAnim = GetAnim(settingsPanelUI);

        if (infoAnim == null) infoAnim = GetAnim(infoPanelUI);
        if (loreAnim == null) loreAnim = GetAnim(lorePanelUI);
        if (teamAnim == null) teamAnim = GetAnim(aboutTeamPanelUI);

        if (mouseAnim == null) mouseAnim = GetAnim(mousePanelUI);
        if (soundAnim == null) soundAnim = GetAnim(soundPanelUI);
        if (graphicsAnim == null) graphicsAnim = GetAnim(graphicsPanelUI);
        if (keyVar1Anim == null) keyVar1Anim = GetAnim(keyVar1PanelUI);
        if (keyVar2Anim == null) keyVar2Anim = GetAnim(keyVar2PanelUI);

        // --- Initial State (Close All) ---
        ClosePanel(settingsPanelUI, settingsAnim);

        ClosePanel(infoPanelUI, infoAnim);
        ClosePanel(lorePanelUI, loreAnim);
        ClosePanel(aboutTeamPanelUI, teamAnim);

        ClosePanel(mousePanelUI, mouseAnim);
        ClosePanel(soundPanelUI, soundAnim);
        ClosePanel(graphicsPanelUI, graphicsAnim);
        ClosePanel(keyVar1PanelUI, keyVar1Anim);
        ClosePanel(keyVar2PanelUI, keyVar2Anim);

        // Main Menu is open
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
    }

    void Update()
    {
        // Back Button Logic (Escape)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Priority 1: Deepest Settings (KeyVar2) -> KeyVar1
            if (IsPanelOpen(keyVar2PanelUI, keyVar2Anim))
            {
                BackToKeyBindingsPage1();
                return;
            }

            // Priority 2: Sub-Settings -> Main Settings
            if (IsPanelOpen(keyVar1PanelUI, keyVar1Anim) ||
                IsPanelOpen(mousePanelUI, mouseAnim) ||
                IsPanelOpen(soundPanelUI, soundAnim) ||
                IsPanelOpen(graphicsPanelUI, graphicsAnim))
            {
                BackToSettings();
                return;
            }

            // Priority 3: Info Sub-panels -> Info
            if (IsPanelOpen(lorePanelUI, loreAnim) || IsPanelOpen(aboutTeamPanelUI, teamAnim))
            {
                BackToInfo();
                return;
            }

            // Priority 4: Main Panels -> Main Menu
            if (IsPanelOpen(infoPanelUI, infoAnim))
            {
                OpenMainMenu(); // Closes Info
                return;
            }

            if (IsPanelOpen(settingsPanelUI, settingsAnim))
            {
                OpenMainMenu(); // Closes Settings
                return;
            }
        }
    }

    // --- Helpers ---
    GameObject FindPanel(params string[] names)
    {
        foreach (var name in names)
        {
            // recursive search might be better but let's stick to this for now + direct children
            var found = transform.Find(name);
            if (found != null) return found.gameObject;
            var go = GameObject.Find(name); // Global search as fallback
            if (go != null) return go;
        }
        return null;
    }

    settingsAnimation GetAnim(GameObject go)
    {
        if (go == null) return null;
        var anim = go.GetComponent<settingsAnimation>();
        if (anim == null) anim = go.GetComponentInChildren<settingsAnimation>(true);
        return anim;
    }

    void ClosePanel(GameObject panel, settingsAnimation anim)
    {
        if (panel != null)
        {
            panel.SetActive(false); // Hard close on start/reset
            if (anim != null) anim.isOpen = false; // Sync state
        }
    }

    void OpenPanelWithAnim(GameObject panel, settingsAnimation anim)
    {
        if (panel != null)
        {
            panel.SetActive(true);
            if (anim != null) anim.Open();
        }
    }

    void ClosePanelWithAnim(settingsAnimation anim)
    {
        if (anim != null) anim.Close();
    }

    bool IsPanelOpen(GameObject panel, settingsAnimation anim)
    {
        // Trust logic state or active state?
        if (panel == null) return false;
        if (anim != null) return anim.isOpen;
        return panel.activeSelf;
    }

    // --- Main Actions ---

    public void Play()
    {
        Inventory.ResetGlobal();
        PlayerStats.ResetStats();
        SceneManager.LoadScene("MainLayout");
    }

    public void OpenSettings()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(true);

        // Close Info System
        CloseInfoSystem();

        // DEEP CLOSE CHECK: If any sub-setting is open, close it and close Settings tree
        if (IsAnySubSettingOpen())
        {
            CloseSettingsSystem();
            return;
        }

        // Toggle Settings
        if (settingsAnim != null && settingsAnim.isOpen)
        {
            // Close
            ClosePanelWithAnim(settingsAnim);
        }
        else
        {
            // Open
            OpenPanelWithAnim(settingsPanelUI, settingsAnim);
        }
    }

    // --- Settings Sub-Panels Navigation ---

    public void OpenMouse()
    {
        // Close Main Settings (or hide it behind?), Open Mouse
        // User request: "panele te posiadaja przycisk back i ma on powodowac powrot" -> implied stack
        // Usually sliding out the parent looks cleaner
        ClosePanelWithAnim(settingsAnim);
        OpenPanelWithAnim(mousePanelUI, mouseAnim);
    }

    public void OpenSound()
    {
        ClosePanelWithAnim(settingsAnim);
        OpenPanelWithAnim(soundPanelUI, soundAnim);
    }

    public void OpenGraphics()
    {
        ClosePanelWithAnim(settingsAnim);
        OpenPanelWithAnim(graphicsPanelUI, graphicsAnim);
    }

    public void OpenKeyBindings() // Var 1
    {
        ClosePanelWithAnim(settingsAnim);
        OpenPanelWithAnim(keyVar1PanelUI, keyVar1Anim);
    }

    public void OpenKeyBindingsPage2() // Var 1 -> Var 2
    {
        ClosePanelWithAnim(keyVar1Anim);
        OpenPanelWithAnim(keyVar2PanelUI, keyVar2Anim);
    }

    public void BackToKeyBindingsPage1() // Var 2 -> Var 1
    {
        ClosePanelWithAnim(keyVar2Anim);
        OpenPanelWithAnim(keyVar1PanelUI, keyVar1Anim);
    }

    public void BackToSettings()
    {
        // Close all sub-panels
        ClosePanelWithAnim(mouseAnim);
        ClosePanelWithAnim(soundAnim);
        ClosePanelWithAnim(graphicsAnim);
        ClosePanelWithAnim(keyVar1Anim);
        // Page 2 usually goes back to Page 1, but if called from Page 2 directly to Settings? 
        // Let's assume BackToSettings cleans everything.
        ClosePanelWithAnim(keyVar2Anim);

        // Open Main Settings
        OpenPanelWithAnim(settingsPanelUI, settingsAnim);
    }

    bool IsAnySubSettingOpen()
    {
        return IsPanelOpen(mousePanelUI, mouseAnim) ||
               IsPanelOpen(soundPanelUI, soundAnim) ||
               IsPanelOpen(graphicsPanelUI, graphicsAnim) ||
               IsPanelOpen(keyVar1PanelUI, keyVar1Anim) ||
               IsPanelOpen(keyVar2PanelUI, keyVar2Anim);
    }

    void CloseSettingsSystem()
    {
        ClosePanelWithAnim(settingsAnim);
        ClosePanelWithAnim(mouseAnim);
        ClosePanelWithAnim(soundAnim);
        ClosePanelWithAnim(graphicsAnim);
        ClosePanelWithAnim(keyVar1Anim);
        ClosePanelWithAnim(keyVar2Anim);
    }

    // --- Info System ---

    public void OpenInfo()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(true);

        // Close Settings System
        CloseSettingsSystem();

        // Check if ANY Info part is open -> Close All (Toggle behavior)
        if (IsPanelOpen(infoPanelUI, infoAnim) || IsPanelOpen(lorePanelUI, loreAnim) || IsPanelOpen(aboutTeamPanelUI, teamAnim))
        {
            CloseInfoSystem();
        }
        else
        {
            // Open Info
            OpenPanelWithAnim(infoPanelUI, infoAnim);
        }
    }

    public void OpenLore()
    {
        ClosePanelWithAnim(infoAnim);
        OpenPanelWithAnim(lorePanelUI, loreAnim);
    }

    public void OpenAboutTeam()
    {
        ClosePanelWithAnim(infoAnim);
        OpenPanelWithAnim(aboutTeamPanelUI, teamAnim);
    }

    public void BackToInfo()
    {
        ClosePanelWithAnim(loreAnim);
        ClosePanelWithAnim(teamAnim);
        OpenPanelWithAnim(infoPanelUI, infoAnim);
    }

    void CloseInfoSystem()
    {
        ClosePanelWithAnim(infoAnim);
        ClosePanelWithAnim(loreAnim);
        ClosePanelWithAnim(teamAnim);
    }

    public void OpenMainMenu()
    {
        // Closes everything
        CloseSettingsSystem();
        CloseInfoSystem();

        if (mainMenuUI != null) mainMenuUI.SetActive(true);
    }

    // Legacy
    public void Settings() => OpenSettings();
    public void QuitGame()
    {
        Debug.Log("Quit");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
