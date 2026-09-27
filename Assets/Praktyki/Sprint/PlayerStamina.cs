using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [Tooltip("Maksymalna stamina (na początku bardzo krótka, np. 30)")]
    [SerializeField] private float maxStamina = 30f;
    [Tooltip("Ile staminy zużywa się na sekundę podczas sprintu")]
    [SerializeField] private float staminaDrainRate = 10f;
    [Tooltip("Ile staminy regeneruje się na sekundę")]
    [SerializeField] private float staminaRegenRate = 8f;
    [Tooltip("Czas w sekundach po sprincie zanim stamina zacznie się regenerować")]
    [SerializeField] private float regenDelay = 1f;

    [Header("Sprint Settings")]
    [Tooltip("Mnożnik prędkości podczas sprintu")]
    [SerializeField] private float sprintSpeedMultiplier = 1.6f;

    public UnityEvent<float> OnStaminaChanged;

    private float currentStamina;
    private float regenTimer;
    private bool isSprinting;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float SprintSpeedMultiplier => sprintSpeedMultiplier;
    public bool IsSprinting => isSprinting;

    private void Awake()
    {
        currentStamina = maxStamina;
    }

    private void Start()
    {
        OnStaminaChanged?.Invoke(GetStaminaPercent());
    }

    private float staminaDrainMultiplier = 1f;

    public float StaminaDrainMultiplier
    {
        get => staminaDrainMultiplier;
        set => staminaDrainMultiplier = Mathf.Max(0.05f, value);
    }

    private float armorStaminaDrainMultiplier = 1f;

    public float ArmorStaminaDrainMultiplier
    {
        get => armorStaminaDrainMultiplier;
        set => armorStaminaDrainMultiplier = Mathf.Max(0.1f, value);
    }

    private void Update()
    {
        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * staminaDrainMultiplier * armorStaminaDrainMultiplier * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isSprinting = false;
            }

            regenTimer = regenDelay;
            OnStaminaChanged?.Invoke(GetStaminaPercent());
        }
        else
        {
            if (regenTimer > 0f)
            {
                regenTimer -= Time.deltaTime;
            }
            else if (currentStamina < EffectiveMaxStamina)
            {
                currentStamina += EffectiveStaminaRegen * Time.deltaTime;
                if (currentStamina > EffectiveMaxStamina)
                {
                    currentStamina = EffectiveMaxStamina;
                }
                OnStaminaChanged?.Invoke(GetStaminaPercent());
            }
        }
    }

    /// <summary>
    /// Sprawdza czy gracz chce i może sprintować. Jeśli tak, ustawia stan isSprinting na true.
    /// </summary>
    /// <param name="isMoving">Czy gracz faktycznie wykonuje ruch</param>
    public bool UpdateSprintState(bool isMoving)
    {
        bool shiftPressed = false;
        if (Keyboard.current != null)
        {
            shiftPressed = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        }

        if (shiftPressed && isMoving && currentStamina > 0f)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        return isSprinting;
    }

    private float bonusMaxStamina = 0f;
    private float bonusStaminaRegen = 0f;

    public float EffectiveMaxStamina => maxStamina + bonusMaxStamina;
    public float EffectiveStaminaRegen => staminaRegenRate + bonusStaminaRegen;

    public void SetBonusMaxStamina(float bonus)
    {
        float diff = bonus - bonusMaxStamina;
        bonusMaxStamina = bonus;
        if (diff > 0)
        {
            currentStamina += diff;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, EffectiveMaxStamina);
        OnStaminaChanged?.Invoke(GetStaminaPercent());
    }

    public void SetBonusStaminaRegen(float bonus)
    {
        bonusStaminaRegen = bonus;
    }

    public float GetStaminaPercent()
    {
        return EffectiveMaxStamina > 0 ? currentStamina / EffectiveMaxStamina : 0f;
    }

    public void IncreaseMaxStamina(float amount)
    {
        maxStamina += amount;
        currentStamina += amount;
        OnStaminaChanged?.Invoke(GetStaminaPercent());
    }

    /// <summary>
    /// Natychmiastowo odnawia staminę gracza do maksymalnej wartości (100%).
    /// </summary>
    public void RestoreStaminaToFull()
    {
        currentStamina = EffectiveMaxStamina;
        regenTimer = 0f;
        OnStaminaChanged?.Invoke(GetStaminaPercent());
    }

    /// <summary>
    /// Odnawia określoną ilość staminy.
    /// </summary>
    public void RestoreStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0f, EffectiveMaxStamina);
        regenTimer = 0f;
        OnStaminaChanged?.Invoke(GetStaminaPercent());
    }

    /// <summary>
    /// Sprawdza czy gracz posiada co najmniej określony procent staminy (np. 0.9 = 90%).
    /// </summary>
    public bool HasStaminaPercent(float minPercent)
    {
        return GetStaminaPercent() >= minPercent;
    }

    /// <summary>
    /// Odejmuje określoną ilość staminy (np. przy ataku bronią).
    /// </summary>
    public bool DrainStamina(float amount)
    {
        if (currentStamina < amount)
        {
            currentStamina = 0f;
            regenTimer = regenDelay;
            OnStaminaChanged?.Invoke(GetStaminaPercent());
            return false;
        }

        currentStamina -= amount;
        regenTimer = regenDelay;
        OnStaminaChanged?.Invoke(GetStaminaPercent());
        return true;
    }

    /// <summary>
    /// Zużywa całą staminę gracza do 0 (np. po potężnym ataku specjalnym).
    /// </summary>
    public void DrainAllStamina(float extraRegenDelay = 0.5f)
    {
        currentStamina = 0f;
        isSprinting = false;
        regenTimer = regenDelay + extraRegenDelay;
        OnStaminaChanged?.Invoke(GetStaminaPercent());
    }
}

