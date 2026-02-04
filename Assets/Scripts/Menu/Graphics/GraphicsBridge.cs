using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicsBridge : MonoBehaviour
{
    [Header("UI - Elementy w tej scenie")]
    public TMP_Dropdown localResDropdown;
    public TMP_Dropdown localWinModeDropdown;
    public Slider localContrastSlider;
    public Slider localGammaSlider;

    [Header("UI - Teksty w tej scenie")]
    public TextMeshProUGUI localContrastText;
    public TextMeshProUGUI localGammaText;

    void Start()
    {
        // Próbujemy znaleŸæ instancjê, jeœli jeszcze nie jest przypisana
        if (Graphics.instance == null)
            Graphics.instance = Object.FindFirstObjectByType<Graphics>();

        if (Graphics.instance != null)
        {
            // AUTOMATYCZNE PO£¥CZENIE:
            // Przekazujemy wszystkie suwaki i dropdowny z tej sceny do g³ównego skryptu
            Graphics.instance.RefreshUIReferences(
                localResDropdown,
                localWinModeDropdown,
                localContrastSlider,
                localGammaSlider,
                localContrastText,
                localGammaText
            );

            Debug.Log("Mostek: Pomyœlnie po³¹czono UI z g³ównym systemem Graphics.");
        }
        else
        {
            Debug.LogWarning("Mostek: Nie znaleziono skryptu Graphics. Uruchom grê od MainMenu!");
        }
    }

    // Funkcja dla przycisku APPLY
    public void ApplyToMainGraphics()
    {
        if (Graphics.instance != null)
        {
            Graphics.instance.ApplySettings();
            Debug.Log("Mostek: Wywo³ano ApplySettings w g³ównym skrypcie.");
        }
        else
        {
            Debug.LogError("B£¥D: Nie mo¿na zastosowaæ ustawieñ - brak skryptu Graphics!");
        }
    }

    // Funkcja dla przycisku DEFAULT (Reset)
    public void ResetInMainGraphics()
    {
        if (Graphics.instance != null)
        {
            Graphics.instance.ResetToDefault();
            Debug.Log("Mostek: Wywo³ano ResetToDefault w g³ównym skrypcie.");
        }
    }
}