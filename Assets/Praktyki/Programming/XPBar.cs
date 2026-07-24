using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro support

public class XPBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private GameObject fillObject; // Reference to Fill or Fill Area GameObject
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI xpValueText;

    [Header("Player Reference")]
    [SerializeField] private PlayerStats playerStats;

    private void Awake()
    {
        // Try to find the slider on the same GameObject if not assigned
        if (xpSlider == null)
        {
            xpSlider = GetComponent<Slider>();
        }

        // Try to find fillObject from slider if not assigned
        if (fillObject == null && xpSlider != null && xpSlider.fillRect != null)
        {
            fillObject = xpSlider.fillRect.gameObject;
        }
    }

    private void OnEnable()
    {
        if (playerStats == null)
        {
            FindPlayerStats();
        }

        if (playerStats != null)
        {
            playerStats.OnStatsChanged.AddListener(UpdateXPBar);
            playerStats.OnLevelUp.AddListener(UpdateXPBar);
            UpdateXPBar();
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatsChanged.RemoveListener(UpdateXPBar);
            playerStats.OnLevelUp.RemoveListener(UpdateXPBar);
        }
    }

    private void Start()
    {
        UpdateXPBar();
    }

    private void FindPlayerStats()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
        else
        {
            playerStats = Object.FindFirstObjectByType<PlayerStats>();
        }
    }

    public void UpdateXPBar()
    {
        if (playerStats == null) return;

        int currentXp = playerStats.Experience;
        int nextLevelXp = playerStats.GetXpForNextLevel();

        // Update Slider value and visibility
        if (xpSlider != null)
        {
            float xpPercent = (float)currentXp / nextLevelXp;
            xpSlider.value = Mathf.Clamp01(xpPercent);

            // Hide the fill graphic when XP is 0 to avoid artifacts/dots, show it when XP > 0
            if (fillObject != null)
            {
                fillObject.SetActive(currentXp > 0);
            }
        }

        // Update Level Text
        if (levelText != null)
        {
            levelText.text = $"Lvl {playerStats.Level}";
        }

        // Update XP Ratio Text (e.g., "75 / 100 XP")
        if (xpValueText != null)
        {
            xpValueText.text = $"{currentXp} / {nextLevelXp} XP";
        }
    }
}
