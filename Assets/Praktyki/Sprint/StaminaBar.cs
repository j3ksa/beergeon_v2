using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private PlayerStamina playerStamina;

    private void OnEnable()
    {
        if (playerStamina != null)
        {
            playerStamina.OnStaminaChanged.AddListener(UpdateStaminaBar);
        }
    }

    private void OnDisable()
    {
        if (playerStamina != null)
        {
            playerStamina.OnStaminaChanged.RemoveListener(UpdateStaminaBar);
        }
    }

    private void Awake()
    {
        if (staminaSlider == null)
        {
            staminaSlider = GetComponent<Slider>();
        }

        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = 1f;
        }
    }

    private void Start()
    {
        if (playerStamina == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerStamina = player.GetComponent<PlayerStamina>();
            }

            if (playerStamina == null)
            {
                playerStamina = FindFirstObjectByType<PlayerStamina>();
            }

            if (playerStamina != null)
            {
                playerStamina.OnStaminaChanged.AddListener(UpdateStaminaBar);
                UpdateStaminaBar(playerStamina.GetStaminaPercent());
            }
        }
        else
        {
            UpdateStaminaBar(playerStamina.GetStaminaPercent());
        }
    }

    public void UpdateStaminaBar(float staminaPercent)
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = staminaPercent;
        }
    }
}
