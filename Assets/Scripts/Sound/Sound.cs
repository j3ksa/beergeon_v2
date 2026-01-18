using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class VolumeControl : MonoBehaviour
{
    [Header("Ustawienia")]
    [SerializeField] private float defaultVolume = 80f; // Standardowa g³oœnoœæ

    [Header("Referencje UI")]
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    [Header("Panel Ustawieñ")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        // Wczytujemy zapisan¹ wartoœæ, a jeœli jej nie ma, u¿ywamy domyœlnej
        float savedVolume = PlayerPrefs.GetFloat("MusicVol", defaultVolume);
        volumeSlider.value = savedVolume;

        UpdateVolume(savedVolume);
    }

    // Wywo³ywane przez Slider
    public void UpdateVolume(float value)
    {
        volumeText.text = Mathf.RoundToInt(value).ToString();
        float dBValue = Mathf.Log10(value / 100f) * 20;
        myMixer.SetFloat("MasterVol", dBValue);

        PlayerPrefs.SetFloat("MusicVol", value);
    }

    // NOWA FUNKCJA: PRZYCISK DEFAULT
    public void ResetToDefault()
    {
        // Ustawiamy suwak na wartoœæ domyœln¹
        // To automatycznie wywo³a UpdateVolume, jeœli masz podpiêty event w Sliderze
        volumeSlider.value = defaultVolume;

        Debug.Log("Przywrócono ustawienia domyœlne: " + defaultVolume);
    }

    public void AcceptSettings()
    {
        PlayerPrefs.Save();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}