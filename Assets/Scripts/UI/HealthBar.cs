using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Kolor Zatrucia")]
    [Tooltip("Kolor paska życia podczas zatrucia")]
    [SerializeField] private Color poisonColor = new Color(0.2f, 0.85f, 0.25f, 1f);

    private static HealthBar instance;
    public static HealthBar Instance => instance;

    private Image fillImage;
    private Color originalFillColor = Color.white;
    private bool hasCachedColor = false;

    private void OnEnable()
    {
        instance = this;
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
        instance = this;
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            CacheFillImage();
        }
    }

    private void CacheFillImage()
    {
        if (hasCachedColor) return;
        if (healthSlider != null && healthSlider.fillRect != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                originalFillColor = fillImage.color;
                hasCachedColor = true;
            }
        }
    }

    public void SetPoisonedVisual(bool isPoisoned)
    {
        CacheFillImage();
        if (fillImage != null)
        {
            fillImage.color = isPoisoned ? poisonColor : originalFillColor;
        }
    }

    public static void SetPoisonState(bool isPoisoned)
    {
        if (instance == null)
        {
            instance = UnityEngine.Object.FindFirstObjectByType<HealthBar>();
        }
        if (instance != null)
        {
            instance.SetPoisonedVisual(isPoisoned);
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
