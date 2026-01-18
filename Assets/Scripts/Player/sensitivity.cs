using UnityEngine;
using UnityEngine.UI;
using TMPro; // NOWE: Biblioteka do obsługi tekstów TextMeshPro

public class Sensitivity : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI valueText; // NOWE: Tu przeciągniesz swój napis z hierarchii
    public float defaultSensitivity = 50f;

    private float tempSensitivity;

    private void Start()
    {
        tempSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);

        slider.value = tempSensitivity;
        UpdateText(tempSensitivity); // NOWE: Ustawienie tekstu na starcie

        slider.onValueChanged.AddListener(ChangeTempSensitivity);
    }

    private void ChangeTempSensitivity(float value)
    {
        tempSensitivity = value;
        UpdateText(value); // NOWE: Aktualizacja tekstu przy ruchu suwakiem
    }

    // NOWE: Pomocnicza funkcja do odświeżania napisu
    private void UpdateText(float value)
    {
        if (valueText != null)
        {
            // "F0" oznacza brak miejsc po przecinku. Jeśli chcesz np. 50.5, zmień na "F1"
            valueText.text = value.ToString("F0");
        }
    }

    public void ResetToDefault()
    {
        slider.value = defaultSensitivity;
        tempSensitivity = defaultSensitivity;
        UpdateText(defaultSensitivity); // NOWE: Aktualizacja przy resecie
    }

    public void ApplySensitivity()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", tempSensitivity);
        PlayerPrefs.Save();

        CameraController cam = FindFirstObjectByType<CameraController>();
        if (cam != null)
        {
            cam.MouseSensitivity = tempSensitivity;
        }
    }
}
