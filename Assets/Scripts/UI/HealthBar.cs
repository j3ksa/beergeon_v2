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
            
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            
        }
        else
        {
            
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
                    
                    playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
                    UpdateHealthBar(playerHealth.GetHealthPercent());
                }
                else
                {
                    
                }
            }
            else
            {
                
            }
        }
        else
        {
             // Ensure we are synced if assigned via Inspector
             
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
