using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References (Auto-Found if null)")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown fullscreenDropdown; // Changed from Toggle
    public Slider qualitySlider;
    public Slider gammaSlider;
    public Slider contrastSlider;

    public TextMeshProUGUI qualityValueText;
    public TextMeshProUGUI gammaValueText;
    public TextMeshProUGUI contrastValueText;

    [Header("Post Processing")]
    public UnityEngine.Rendering.Volume globalVolume;

    // Derived components are now fetched on demand to avoid MissingReferenceException with cached ScriptableObjects

    // Internal state
    private int pendingResolutionIndex;
    private int pendingQualityIndex;
    private int pendingFullscreenMode; // 0=Exclusive, 1=FullscreenWindow, 2=Windowed

    // Pending floats for robust saving (even if Volume is missing)
    private float pendingGamma;
    private float pendingContrast;

    private Resolution[] resolutions;

    // PlayerPrefs Keys
    private const string PREF_QUALITY = "Settings_Quality";
    private const string PREF_RES_INDEX = "Settings_ResIndex";
    private const string PREF_FULLSCREEN = "Settings_FullscreenMode";
    private const string PREF_GAMMA = "Settings_Gamma";
    private const string PREF_CONTRAST = "Settings_Contrast";


    void Start()
    {
        // --- AUTO WIRING SECTION ---
        if (qualitySlider == null) qualitySlider = FindSliderByName("Quality");

        // Improved search for Gamma (English + Polish + typos)
        if (gammaSlider == null)
        {
            gammaSlider = FindSliderByName("Gamma");
            if (gammaSlider == null) gammaSlider = FindSliderByName("Jasność"); // Polish
            if (gammaSlider == null) gammaSlider = FindSliderByName("Jasnosc"); // Polish no accents
            if (gammaSlider == null) gammaSlider = FindSliderByName("Brightness");
        }

        if (contrastSlider == null)
        {
            contrastSlider = FindSliderByName("Contrast");
            if (contrastSlider == null) contrastSlider = FindSliderByName("Kontrast"); // Polish
        }

        // Enforce user-requested limits
        // Enforce user-requested limits
        if (gammaSlider != null) { gammaSlider.minValue = -0.5f; gammaSlider.maxValue = 0.5f; }
        if (contrastSlider != null) { contrastSlider.minValue = -99f; contrastSlider.maxValue = 99f; }

        if (resolutionDropdown == null) resolutionDropdown = FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(d => d.name.Contains("Resolution"));
        if (fullscreenDropdown == null) fullscreenDropdown = FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(d => d.name.Contains("Screen") || d.name.Contains("Mode")); // "Screen Mode" or "Fullscreen Mode"

        if (qualityValueText == null) qualityValueText = FindTextByName("Quality");
        if (gammaValueText == null) gammaValueText = FindTextByName("Gamma");
        if (contrastValueText == null) contrastValueText = FindTextByName("Contrast");

        Button applyButton = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(b => b.name.Contains("Apply") || b.name.Contains("Zastosuj"));
        if (applyButton != null) { applyButton.onClick.RemoveAllListeners(); applyButton.onClick.AddListener(ApplyGraphics); }

        Button defaultButton = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(b => b.name.Contains("Default") || b.name.Contains("Reset"));
        if (defaultButton != null) { defaultButton.onClick.RemoveAllListeners(); defaultButton.onClick.AddListener(ResetToDefaults); }

        // Find Quit Button in Settings (if exists, e.g. "Quit to Menu")
        Button quitButton = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(b => b.name.Contains("Quit") || b.name.Contains("Exit"));
        if (quitButton != null) { quitButton.onClick.RemoveAllListeners(); quitButton.onClick.AddListener(QuitToMenu); }


        if (qualitySlider != null) { qualitySlider.onValueChanged.RemoveAllListeners(); qualitySlider.onValueChanged.AddListener(SetQuality); }
        if (gammaSlider != null) { gammaSlider.onValueChanged.RemoveAllListeners(); gammaSlider.onValueChanged.AddListener(SetGamma); }
        if (contrastSlider != null) { contrastSlider.onValueChanged.RemoveAllListeners(); contrastSlider.onValueChanged.AddListener(SetContrast); }
        if (resolutionDropdown != null) { resolutionDropdown.onValueChanged.RemoveAllListeners(); resolutionDropdown.onValueChanged.AddListener(SetResolution); }
        if (fullscreenDropdown != null) { fullscreenDropdown.onValueChanged.RemoveAllListeners(); fullscreenDropdown.onValueChanged.AddListener(SetFullscreenMode); }

        Debug.Log($"[SettingsMenu] Auto-Wiring Complete.");


        // --- LOGIC INIT ---
        resolutions = Screen.resolutions.Select(r => new Resolution { width = r.width, height = r.height }).Distinct().ToArray();

        // Prepare Resolution Dropdown
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            for (int i = 0; i < resolutions.Length; i++)
            {
                options.Add(resolutions[i].width + " x " + resolutions[i].height);
            }
            resolutionDropdown.AddOptions(options);
        }

        // Prepare Fullscreen Dropdown (if empty)
        if (fullscreenDropdown != null && fullscreenDropdown.options.Count == 0)
        {
            fullscreenDropdown.ClearOptions();
            fullscreenDropdown.AddOptions(new List<string> { "Exclusive Fullscreen", "Fullscreen Window", "Windowed" });
        }

        // Setup Volume
        // Setup Volume
        if (globalVolume == null) globalVolume = FindFirstObjectByType<UnityEngine.Rendering.Volume>();

        // We verify volume exists, but we don't cache components anymore.
        if (globalVolume == null)
        {
            // Try one more time? Or just log.
            // Debug.LogWarning("[SettingsMenu] Global Volume not found in scene.");
        }

        LoadSettings();

        // Fix: Ensure the Settings Menu doesn't stay open over the game if not in the Settings scene
        if (SceneManager.GetActiveScene().name != "Settings")
        {
            // Only auto-hide if we are not the PauseMenuUI itself (check parent)
            // Use a safe check
            if (gameObject.activeInHierarchy) gameObject.SetActive(false);
        }
    }

    private Slider FindSliderByName(string partialName)
    {
        // Try to find by partial name (case insensitive could be better but Contains is case sensitive)
        // Let's make it case insensitive for robustness
        return FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(s => s.name.IndexOf(partialName, System.StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private TextMeshProUGUI FindTextByName(string partialName)
    {
        return FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name.Contains(partialName) && (t.name.Contains("Value") || t.name.Contains("Text")));
    }

    // --- LOGIC ---

    public void SetResolution(int resolutionIndex)
    {
        pendingResolutionIndex = resolutionIndex;
    }

    public void SetFullscreenMode(int modeIndex)
    {
        // 0 = Exclusive, 1 = FullscreenWindow, 2 = Windowed
        pendingFullscreenMode = modeIndex;
    }

    public void SetQuality(float qualityIndex)
    {
        int rounded = Mathf.RoundToInt(qualityIndex);
        pendingQualityIndex = rounded;
        if (qualityValueText != null) qualityValueText.text = rounded.ToString();
    }

    public void SetGamma(float val)
    {
        val = Mathf.Clamp(val, -0.5f, 0.5f); // Updated limits: -0.5 to 0.5
        pendingGamma = val;
        if (gammaValueText != null) gammaValueText.text = val.ToString("F1");
        // Use Global Gamma Controller
        if (GammaController.Instance != null)
        {
            GammaController.Instance.SetGamma(val);
        }
        else
        {
            var gc = FindFirstObjectByType<GammaController>();
            if (gc == null)
            {
                GameObject go = new GameObject("GammaController_Auto");
                gc = go.AddComponent<GammaController>();
            }
            gc.SetGamma(val);
        }
    }

    public void SetContrast(float val)
    {
        val = Mathf.Clamp(val, -99f, 99f); // Updated limits: -99 to 99
        pendingContrast = val;
        if (contrastValueText != null) contrastValueText.text = val.ToString("F1");
        // Use Global Gamma Controller for consistency
        if (GammaController.Instance != null)
        {
            GammaController.Instance.SetContrast(val);
        }
        else
        {
            var gc = FindFirstObjectByType<GammaController>();
            if (gc == null)
            {
                GameObject go = new GameObject("GammaController_Auto");
                gc = go.AddComponent<GammaController>();
            }
            gc.SetContrast(val);
        }
    }

    public void ResetToDefaults()
    {
        SetQuality(2);
        SetFullscreenMode(0); // Exclusive
        SetResolution(resolutions.Length - 1);
        SetGamma(0f);
        SetContrast(0f);

        ApplyGraphics();
        UpdateVisuals();
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void ApplyGraphics()
    {
        // Apply Resolution & Fullscreen
        if (resolutions != null && resolutions.Length > pendingResolutionIndex && pendingResolutionIndex >= 0)
        {
            Resolution res = resolutions[pendingResolutionIndex];
            FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
            if (pendingFullscreenMode == 1) mode = FullScreenMode.FullScreenWindow;
            else if (pendingFullscreenMode == 2) mode = FullScreenMode.Windowed;

            Screen.SetResolution(res.width, res.height, mode);
        }
        QualitySettings.SetQualityLevel(pendingQualityIndex);

        // Save preferences from PENDING values (reliable)
        PlayerPrefs.SetInt(PREF_QUALITY, pendingQualityIndex);
        PlayerPrefs.SetInt(PREF_RES_INDEX, pendingResolutionIndex);
        PlayerPrefs.SetInt(PREF_FULLSCREEN, pendingFullscreenMode);
        // Gamma/Contrast are applied real-time usually, but let's save them
        // We use pending vars which are the source of truth
        PlayerPrefs.SetFloat(PREF_GAMMA, pendingGamma);
        PlayerPrefs.SetFloat(PREF_CONTRAST, pendingContrast);

        PlayerPrefs.Save();

        Debug.Log($"[SettingsMenu] Settings Applied and Saved! Gamma={pendingGamma}, Contrast={pendingContrast}");
    }

    private void LoadSettings()
    {
        // Load Defaults or Prefs
        pendingQualityIndex = PlayerPrefs.GetInt(PREF_QUALITY, QualitySettings.GetQualityLevel());
        pendingFullscreenMode = PlayerPrefs.GetInt(PREF_FULLSCREEN, 0); // Default Exclusive

        // Resolution matches current or saved
        int savedResIndex = PlayerPrefs.GetInt(PREF_RES_INDEX, -1);
        if (savedResIndex >= 0 && savedResIndex < resolutions.Length) pendingResolutionIndex = savedResIndex;
        else
        {
            // Find current
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                {
                    pendingResolutionIndex = i; break;
                }
            }
        }

        pendingGamma = PlayerPrefs.GetFloat(PREF_GAMMA, 0f); // Default 0
        pendingContrast = PlayerPrefs.GetFloat(PREF_CONTRAST, 0f); // Default 0

        SetGamma(pendingGamma);
        SetContrast(pendingContrast);

        UpdateVisuals();

        // Enforce loaded settings immediately!
        ApplyGraphics();
    }

    private void UpdateVisuals()
    {
        if (qualitySlider != null) { qualitySlider.value = pendingQualityIndex; SetQuality(pendingQualityIndex); }
        if (resolutionDropdown != null) { resolutionDropdown.value = pendingResolutionIndex; resolutionDropdown.RefreshShownValue(); }
        if (fullscreenDropdown != null) { fullscreenDropdown.value = pendingFullscreenMode; fullscreenDropdown.RefreshShownValue(); }

        // Gamma/Contrast already updated via SetGamma/SetContrast which update texts
        // But need to update slider positions
        if (gammaSlider != null) gammaSlider.value = pendingGamma;
        if (contrastSlider != null) contrastSlider.value = pendingContrast;
    }
}
