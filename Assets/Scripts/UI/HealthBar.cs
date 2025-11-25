using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private PlayerHealth playerHealth;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }

    private void Awake()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }

        if (healthSlider != null)
        {
            Debug.Log($"HealthBar: Setting slider range. Old Max: {healthSlider.maxValue}");
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            Debug.Log($"HealthBar: Slider range set to 0-1. New Max: {healthSlider.maxValue}");
        }
        else
        {
            Debug.LogError("HealthBar: Slider component not assigned and not found on GameObject!");
        }
    }

    private void Start()
    {
        // Optional: Auto-find PlayerHealth if not assigned
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    Debug.Log("HealthBar: PlayerHealth found via tag.");
                    playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
                    UpdateHealthBar(playerHealth.GetHealthPercent());
                }
                else
                {
                    Debug.LogError("HealthBar: Player object found, but PlayerHealth component is missing!");
                }
            }
            else
            {
                Debug.LogError("HealthBar: No object with tag 'Player' found!");
            }
        }
        else
        {
             // Ensure we are synced if assigned via Inspector
             Debug.Log("HealthBar: PlayerHealth assigned via Inspector.");
             UpdateHealthBar(playerHealth.GetHealthPercent());
        }
    }

    public void UpdateHealthBar(float healthPercent)
    {
        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }
    }
}
