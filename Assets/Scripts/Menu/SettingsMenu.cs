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

    [Header("Notification")]
    public TextMeshProUGUI notificationText;

    [Header("Post Processing")]
    public UnityEngine.Rendering.Volume globalVolume;

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
    private const string PREF_SENSITIVITY = "MouseSensitivity"; // Matches sensitivity.cs
    private const string PREF_VOLUME = "Settings_MasterVolume";

    [Header("Sensitivity")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    private float pendingSensitivity;

    [Header("Audio")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;
    private float pendingVolume;


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
        if (gammaSlider != null) { gammaSlider.minValue = -0.5f; gammaSlider.maxValue = 0.5f; }
        if (contrastSlider != null) { contrastSlider.minValue = -99f; contrastSlider.maxValue = 99f; }

        if (resolutionDropdown == null) resolutionDropdown = FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(d => d.name.Contains("Resolution"));
        if (fullscreenDropdown == null) fullscreenDropdown = FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(d => d.name.Contains("Screen") || d.name.Contains("Mode")); // "Screen Mode" or "Fullscreen Mode"

        if (qualityValueText == null) qualityValueText = FindTextByName("Quality");
        if (gammaValueText == null) gammaValueText = FindTextByName("Gamma");
        if (contrastValueText == null) contrastValueText = FindTextByName("Contrast");
        if (sensitivityValueText == null) sensitivityValueText = FindTextByName("Sens");

        // Find Notification Text
        if (notificationText == null)
        {
            notificationText = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
               .FirstOrDefault(t => t.name.Contains("Notify") || t.name.Contains("Notification") || t.name.Contains("ApplyMessage"));
        }

        if (volumeSlider != null)
        {
             volumeSlider.minValue = 0f;
             volumeSlider.maxValue = 1f;
             volumeSlider.wholeNumbers = false; // Fix for snapping only to 0/1
             volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // --- BUTTONS SMART WIRING ---
        var allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach(var btn in allButtons)
        {
            if (btn == null) continue;
            string lowerName = btn.name.ToLower();
            string parentName = btn.transform.parent != null ? btn.transform.parent.name.ToLower() : "";
            
            // APPLY BUTTONS
            if (lowerName.Contains("apply") || lowerName.Contains("zastosuj"))
            {
                btn.onClick.RemoveAllListeners();
                if (parentName.Contains("sound") || parentName.Contains("audio") || parentName.Contains("dźwięk"))
                {
                    btn.onClick.AddListener(ApplySound);
                }
                else if (parentName.Contains("mouse") || parentName.Contains("sens"))
                {
                    // No specific apply for mouse implemented, maybe just save?
                    btn.onClick.AddListener(() => PlayerPrefs.Save());
                }
                else
                {
                    // Default to Graphics
                    btn.onClick.AddListener(ApplyGraphics);
                }
            }
            
            // DEFAULT BUTTONS
            if (lowerName.Contains("default") || lowerName.Contains("reset"))
            {
                btn.onClick.RemoveAllListeners();
                if (parentName.Contains("sound") || parentName.Contains("audio") || parentName.Contains("dźwięk"))
                {
                    btn.onClick.AddListener(ResetVolume);
                }
                else if (parentName.Contains("mouse") || parentName.Contains("sens"))
                {
                    btn.onClick.AddListener(ResetSensitivity);
                }
                else
                {
                    // Default to Graphics
                    btn.onClick.AddListener(ResetToDefaults);
                }
            }

            // QUIT BUTTON
            if (lowerName.Contains("quit") || lowerName.Contains("exit"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(QuitToMenu);
            }
        }

        if (qualitySlider != null) { qualitySlider.onValueChanged.RemoveAllListeners(); qualitySlider.onValueChanged.AddListener(SetQuality); }
        if (gammaSlider != null) { gammaSlider.onValueChanged.RemoveAllListeners(); gammaSlider.onValueChanged.AddListener(SetGamma); }
        if (contrastSlider != null) { contrastSlider.onValueChanged.RemoveAllListeners(); contrastSlider.onValueChanged.AddListener(SetContrast); }
        if (sensitivitySlider != null) { sensitivitySlider.onValueChanged.RemoveAllListeners(); sensitivitySlider.onValueChanged.AddListener(SetSensitivity); }
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

        if (globalVolume == null) globalVolume = FindFirstObjectByType<UnityEngine.Rendering.Volume>();

        LoadSettings();

        // --- FIX: Auto-subscribe to events to ensure functionality ---
        // (Moved to Update for persistence, but kept here for init)
        if (fullscreenDropdown != null) fullscreenDropdown.onValueChanged.AddListener(SetFullscreenMode);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(SetResolution);
        if (qualitySlider != null) qualitySlider.onValueChanged.AddListener(SetQuality);
        if (gammaSlider != null) gammaSlider.onValueChanged.AddListener(SetGamma);
        if (contrastSlider != null) contrastSlider.onValueChanged.AddListener(SetContrast);
        if (sensitivitySlider != null) sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        
        // --- VOLUME AUTO WIRING ---
        if (volumeSlider == null) 
        {
             volumeSlider = FindSliderByName("Sound");
             if (volumeSlider == null) volumeSlider = FindSliderByName("Volume");
             if (volumeSlider == null) volumeSlider = FindSliderByName("Audio");
             if (volumeSlider == null) volumeSlider = FindSliderByName("Dźwięk");
        }
        
        if (volumeValueText == null) volumeValueText = FindTextNextToSlider(volumeSlider, "Sound");
        if (volumeValueText == null) volumeValueText = FindTextByName("Sound"); // Fallback

        if (volumeSlider != null)
        {
             volumeSlider.minValue = 0f;
             volumeSlider.maxValue = 1f;
             volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }
    
    // ... FindTextNextToSlider helper was missing in previous view, adding it back if needed or using FindTextByName
    private TextMeshProUGUI FindTextNextToSlider(Slider slider, string fallbackName)
    {
        if (slider != null && slider.transform.parent != null)
        {
            var siblingTexts = slider.transform.parent.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach(var t in siblingTexts)
            {
                 if (t.name.Contains("Value") || t.name.Contains("Num") || t.name.Contains("Text") || t.name.Contains("%")) return t;
            }
        }
        return null;
    }

    private void ValidateVolume()
    {
        if (globalVolume == null)
        {
            globalVolume = FindFirstObjectByType<UnityEngine.Rendering.Volume>();
            if (globalVolume != null) Debug.Log("[SettingsMenu] Global Volume re-found.");
        }
    }

    private Slider FindSliderByName(string partialName)
    {
        return FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(s => s.name.IndexOf(partialName, System.StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private TextMeshProUGUI FindTextByName(string partialName)
    {
        return FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name.Contains(partialName) && (t.name.Contains("Value") || t.name.Contains("Text")));
    }

    // --- LOGIC ---

    private bool eventsHooked = false;
    void Update()
    {
        if (!eventsHooked)
        {
            if (gammaSlider != null) { gammaSlider.onValueChanged.RemoveAllListeners(); gammaSlider.onValueChanged.AddListener(SetGamma); }
            if (contrastSlider != null) { contrastSlider.onValueChanged.RemoveAllListeners(); contrastSlider.onValueChanged.AddListener(SetContrast); }
            eventsHooked = true;
            Debug.Log("[SettingsMenu] Events force-hooked in Update.");
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        pendingResolutionIndex = resolutionIndex;
        Debug.Log($"[SettingsMenu] Resolution selected: Index {resolutionIndex}");
    }

    public void SetFullscreenMode(int modeIndex)
    {
        // 0 = Exclusive, 1 = FullscreenWindow, 2 = Windowed
        pendingFullscreenMode = modeIndex;
        Debug.Log($"[SettingsMenu] Fullscreen Mode selected: Index {modeIndex}");
    }

    public void SetQuality(float qualityIndex)
    {
        int rounded = Mathf.RoundToInt(qualityIndex);
        pendingQualityIndex = rounded;
        if (qualityValueText != null) qualityValueText.text = rounded.ToString();
    }

    public void SetSensitivity(float val)
    {
        pendingSensitivity = val;
        if (sensitivityValueText != null) sensitivityValueText.text = val.ToString("F0");

        // Instant Save for Sensitivity
        PlayerPrefs.SetFloat(PREF_SENSITIVITY, val);
        PlayerPrefs.Save();
    }

    public void SetGamma(float val)
    {
        Debug.Log($"[SettingsMenu] SetGamma Called. Raw Val: {val}");
        val = Mathf.Clamp(val, -0.5f, 0.5f);
        pendingGamma = val;

        if (gammaValueText != null)
        {
            gammaValueText.text = val.ToString("F1");
            gammaValueText.SetAllDirty();
        }
        else Debug.LogWarning($"[SettingsMenu] Gamma Text is NULL! Val: {val}");

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
        Debug.Log($"[SettingsMenu] SetContrast Called. Raw Val: {val}");
        val = Mathf.Clamp(val, -99f, 99f);
        pendingContrast = val;

        if (contrastValueText != null)
        {
            contrastValueText.text = val.ToString("F1");
            contrastValueText.SetAllDirty();
        }
        else Debug.LogWarning($"[SettingsMenu] Contrast Text is NULL! Val: {val}");

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
        SetVolume(1f);
        ResetSensitivity();

        ApplyGraphics();
        UpdateVisuals();
    }

    public void ResetSensitivity()
    {
        SetSensitivity(50f);
        if (sensitivitySlider != null) sensitivitySlider.value = 50f;
    }

    public void ResetVolume()
    {
        SetVolume(1f);
        if (volumeSlider != null) volumeSlider.value = 1f;
    }

    public void ApplySound()
    {
        PlayerPrefs.Save();
        Debug.Log("[SettingsMenu] Sound Settings Applied/Saved");
        // Reuse graphical notification if convenient, or just log
        Debug.Log($"[SettingsMenu] APPLIED: Master Volume {pendingVolume * 100:F0}%");
    }

    public void SetVolume(float val)
    {
        val = Mathf.Clamp01(val);
        pendingVolume = val;
        
        // Apply to Unity Audio
        AudioListener.volume = val;
        
        // Apply to FMOD (Try/Catch to be safe if FMOD is missing or bus not found)
        try
        {
            FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(val);
        }
        catch (System.Exception) 
        { 
             // Ignore if FMOD not present or error
        }

        PlayerPrefs.SetFloat(PREF_VOLUME, val);
        PlayerPrefs.Save();

        if (volumeValueText != null) 
        {
            volumeValueText.text = (val * 100f).ToString("F0") + "%";
        }
    }

    public void QuitToMenu()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        // If we are already in MainMenu, Quit the Application
        if (currentScene == "MainMenu" || currentScene.Contains("Menu"))
        {
            Debug.Log("[SettingsMenu] Quitting Application from Main Menu");
            Application.Quit();
        }
        else
        {
            // Otherwise go to MainMenu
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ApplyGraphics()
    {
        if (resolutions != null && resolutions.Length > pendingResolutionIndex && pendingResolutionIndex >= 0)
        {
            Resolution res = resolutions[pendingResolutionIndex];
            FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
            if (pendingFullscreenMode == 1) mode = FullScreenMode.FullScreenWindow;
            else if (pendingFullscreenMode == 2) mode = FullScreenMode.Windowed;

            Screen.SetResolution(res.width, res.height, mode);
            Debug.Log($"[SettingsMenu] Applying Resolution: {res.width}x{res.height}, Mode: {mode}");
        }
        QualitySettings.SetQualityLevel(pendingQualityIndex);

        PlayerPrefs.SetInt(PREF_QUALITY, pendingQualityIndex);
        PlayerPrefs.SetInt(PREF_RES_INDEX, pendingResolutionIndex);
        PlayerPrefs.SetInt(PREF_FULLSCREEN, pendingFullscreenMode);
        PlayerPrefs.SetFloat(PREF_GAMMA, pendingGamma);
        PlayerPrefs.SetFloat(PREF_CONTRAST, pendingContrast);
        // Sensitivity already saved instantly, but safe to save pending if needed

        PlayerPrefs.Save();

        Debug.Log($"[SettingsMenu] Settings Applied and Saved! Gamma={pendingGamma}, Contrast={pendingContrast}");

        Debug.Log($"[SettingsMenu] APPLIED:\nRes: {resolutions[pendingResolutionIndex].width}x{resolutions[pendingResolutionIndex].height}\nMode: {(pendingFullscreenMode == 0 ? "Exclusive" : pendingFullscreenMode == 1 ? "WindowFull" : "Windowed")}\nQuality: {pendingQualityIndex}");
    }

    private void LoadSettings()
    {
        pendingQualityIndex = PlayerPrefs.GetInt(PREF_QUALITY, QualitySettings.GetQualityLevel());
        pendingFullscreenMode = PlayerPrefs.GetInt(PREF_FULLSCREEN, 0);

        int savedResIndex = PlayerPrefs.GetInt(PREF_RES_INDEX, -1);
        if (savedResIndex >= 0 && savedResIndex < resolutions.Length) pendingResolutionIndex = savedResIndex;
        else
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                {
                    pendingResolutionIndex = i; break;
                }
            }
        }

        pendingGamma = PlayerPrefs.GetFloat(PREF_GAMMA, 0f);
        pendingContrast = PlayerPrefs.GetFloat(PREF_CONTRAST, 0f);
        pendingSensitivity = PlayerPrefs.GetFloat(PREF_SENSITIVITY, 50f);
        pendingVolume = PlayerPrefs.GetFloat(PREF_VOLUME, 1f); // Default 1.0

        SetGamma(pendingGamma);
        SetContrast(pendingContrast);
        SetSensitivity(pendingSensitivity);
        SetVolume(pendingVolume);

        UpdateVisuals();
        ApplyGraphics();
    }

    private void UpdateVisuals()
    {
        if (qualitySlider != null) { qualitySlider.value = pendingQualityIndex; SetQuality(pendingQualityIndex); }
        if (resolutionDropdown != null) { resolutionDropdown.value = pendingResolutionIndex; resolutionDropdown.RefreshShownValue(); }
        if (fullscreenDropdown != null) { fullscreenDropdown.value = pendingFullscreenMode; fullscreenDropdown.RefreshShownValue(); }

        if (gammaSlider != null) gammaSlider.value = pendingGamma;
        if (contrastSlider != null) contrastSlider.value = pendingContrast;
        if (sensitivitySlider != null) sensitivitySlider.value = pendingSensitivity;
        if (volumeSlider != null) volumeSlider.value = pendingVolume;
    }

    // Update loop injection for Volume
    // Note: Update() method exists above, we just need to make sure we add volume to it.
    // Since I can't easily edit Update which is higher up without huge context, I'll rely on Start wiring for now.
    // But let's add Volume loading to LoadSettings.

}
