using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Graphics : MonoBehaviour
{
    public static Graphics instance;

    [Header("UI - Elementy")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown windowModeDropdown;
    public Slider contrastSlider;
    public Slider gammaSlider;

    [Header("UI - Liczniki Wartoœci")]
    public TextMeshProUGUI contrastText;
    public TextMeshProUGUI gammaText;

    [Header("Post Processing")]
    public Volume globalVolume;
    private ColorAdjustments colorAdjustments;

    private Resolution[] resolutions;
    private int selectedResolutionIndex;

    void Awake()
    {
        // Singleton - sprawia, ¿e skrypt ¿yje przez ca³¹ grê
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return; // Zatrzymujemy dalsze wykonywanie Awake dla duplikatu
        }
    }

    void Start()
    {
        InitializeSettings();
    }

    // TA FUNKCJA JEST KLUCZOWA: Wywo³aj j¹ z GraphicsBridge w scenie Settings!
    public void RefreshUIReferences(TMP_Dropdown resDip, TMP_Dropdown winDip, Slider conSli, Slider gamSli, TextMeshProUGUI conTex, TextMeshProUGUI gamTex)
    {
        resolutionDropdown = resDip;
        windowModeDropdown = winDip;
        contrastSlider = conSli;
        gammaSlider = gamSli;
        contrastText = conTex;
        gammaText = gamTex;

        // Po podpiêciu nowych elementów UI, odœwie¿ ich stan na podstawie zapisanych danych
        InitializeSettings();
    }

    public void InitializeSettings()
    {
        // 1. Rozdzielczoœci
        resolutions = Screen.resolutions;
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            int currentResIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                options.Add(resolutions[i].width + " x " + resolutions[i].height);
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                    currentResIndex = i;
            }
            resolutionDropdown.AddOptions(options);

            // Wczytanie zapisu
            selectedResolutionIndex = PlayerPrefs.GetInt("ResIndex", currentResIndex);
            resolutionDropdown.value = selectedResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        // 2. Tryb okna
        if (windowModeDropdown != null)
        {
            windowModeDropdown.value = PlayerPrefs.GetInt("WindowModeIndex", 0);
            windowModeDropdown.RefreshShownValue();
        }

        LoadSavedVisuals();
        ApplySettings();
    }

    public void FindVolumeInScene()
    {
        // SprawdŸ czy nazwa obiektu w Unity to na pewno "Post-Procesing" (z jednym 's')
        GameObject currentVolObject = GameObject.Find("Post-Procesing");
        if (currentVolObject != null)
        {
            globalVolume = currentVolObject.GetComponent<Volume>();
            if (globalVolume != null && globalVolume.profile != null)
            {
                globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
            }
        }
    }

    public void LoadSavedVisuals()
    {
        FindVolumeInScene();

        float savedContrast = PlayerPrefs.GetFloat("SavedContrast", 0f);
        float savedGamma = PlayerPrefs.GetFloat("SavedGamma", 0f);

        if (contrastSlider != null)
        {
            contrastSlider.value = savedContrast;
            UpdateContrastValue(savedContrast);
        }
        if (gammaSlider != null)
        {
            gammaSlider.value = savedGamma;
            UpdateGammaValue(savedGamma);
        }
    }

    public void UpdateContrastValue(float val) { if (contrastText != null) contrastText.text = val.ToString("F0"); }
    public void UpdateGammaValue(float val) { if (gammaText != null) gammaText.text = val.ToString("F1"); }

    public void ApplySettings()
    {
        // 1. NAPRAWIONA KOLEJNOŒÆ ROZDZIELCZOŒCI
        if (resolutions != null && resolutions.Length > 0 && resolutionDropdown != null)
        {
            // Pobieramy now¹ wartoœæ z UI
            selectedResolutionIndex = resolutionDropdown.value;
            Resolution res = resolutions[selectedResolutionIndex];

            FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
            if (windowModeDropdown.value == 1) mode = FullScreenMode.Windowed;
            if (windowModeDropdown.value == 2) mode = FullScreenMode.FullScreenWindow;

            Screen.SetResolution(res.width, res.height, mode);

            PlayerPrefs.SetInt("ResIndex", selectedResolutionIndex);
            PlayerPrefs.SetInt("WindowModeIndex", windowModeDropdown.value);
        }

        // 2. Odœwie¿enie po³¹czenia z Post-Processingiem
        FindVolumeInScene();

        // 3. Aplikowanie suwaków
        if (colorAdjustments != null && contrastSlider != null && gammaSlider != null)
        {
            colorAdjustments.contrast.Override(contrastSlider.value);
            colorAdjustments.postExposure.Override(gammaSlider.value);

            PlayerPrefs.SetFloat("SavedContrast", contrastSlider.value);
            PlayerPrefs.SetFloat("SavedGamma", gammaSlider.value);
        }

        PlayerPrefs.Save();
        Debug.Log("Ustawienia zastosowane.");
    }

    public void ResetToDefault()
    {
        if (contrastSlider != null) contrastSlider.value = 0f;
        if (gammaSlider != null) gammaSlider.value = 0f;
        UpdateContrastValue(0f);
        UpdateGammaValue(0f);

        if (resolutions != null && resolutions.Length > 0 && resolutionDropdown != null)
        {
            resolutionDropdown.value = resolutions.Length - 1;
            resolutionDropdown.RefreshShownValue();
        }

        if (windowModeDropdown != null)
        {
            windowModeDropdown.value = 0;
            windowModeDropdown.RefreshShownValue();
        }

        ApplySettings();
    }
}