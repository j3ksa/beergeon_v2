using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Beergeon.Player; // Required for HandRenderingSetup

public class PlayerHand : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Camera playerCamera;
    
    [Header("Weapon Settings")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask hitLayers;

    [Header("Bobbing Settings")]
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private float bobAmount = 0.05f;
    [SerializeField] private CharacterController characterController;

    [Header("Input")]
    [SerializeField] private InputActionReference attackAction;
    
    [Header("Dostrajanie Pozycji Broni (Live Inspector Tweak)")]
    [Tooltip("Możesz zmieniać te wartości bezpośrednio w Inspektorze w trakcie gry na obiekcie WeaponHolder (PlayerHand)!")]
    public Vector3 livePositionOffset = Vector3.zero;
    public Vector3 liveRotationOffset = Vector3.zero;

    // Reference to HandRenderingSetup to control rotation without conflict
    private HandRenderingSetup handRenderingSetup;

    private Vector3 defaultLocalPos;
    private float defaultYPos;
    private float timer = 0;
    private bool isAttacking = false;
    private float lastAttackTime = 0;

    // Current Item
    private EquippableItem currentItem;
    public EquippableItem CurrentItem => currentItem;
    private PlayerController playerController;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (handRenderingSetup == null) handRenderingSetup = GetComponentInParent<HandRenderingSetup>() ?? GetComponent<HandRenderingSetup>();

        defaultLocalPos = transform.localPosition;
        defaultYPos = defaultLocalPos.y;
    }

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCamera == null) playerCamera =  Camera.main;
        
        // Try to find HandRenderingSetup
        if (handRenderingSetup == null) handRenderingSetup = GetComponentInParent<HandRenderingSetup>() ?? GetComponent<HandRenderingSetup>();
        
        if (defaultLocalPos == Vector3.zero)
        {
            defaultLocalPos = transform.localPosition;
            defaultYPos = defaultLocalPos.y;
        }
        
        // Try to find stats if not assigned
        if (playerStats == null)
        {
            playerStats = GetComponentInParent<PlayerStats>();
        }
        
        // Try to find character controller for bobbing
        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();
        }

        // Initialize with generic idle
        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }

        // Subscribe to Inventory
        if (Inventory.instance != null)
        {
            Inventory.instance.OnActiveSlotChanged.AddListener(UpdateHandVisuals);
            UpdateHandVisuals(); // Initial check
        }
        else
        {
            Debug.LogWarning("PlayerHand: Inventory instance not found.");
        }
    }

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.action.Enable();
            attackAction.action.performed += OnAttack;
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.action.performed -= OnAttack;
            attackAction.action.Disable();
        }

        if (currentItem != null)
        {
            if (playerController == null) playerController = GetComponentInParent<PlayerController>();
            currentItem.OnUnequipped(playerController, this);
        }
    }

    private void Update()
    {
        HandleBobbing();
        HandleSecondaryInput();
        HandleDirectAttackFallback();
    }

    private bool IsInputBlocked()
    {
        return Time.timeScale == 0f || Praktyki.EQ.MinecraftInventoryUI.IsOpen;
    }

    private void EnsureCursorLocked()
    {
        if (Cursor.lockState != CursorLockMode.Locked && Time.timeScale > 0f)
        {
            if (Praktyki.EQ.MinecraftInventoryUI.IsOpen) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleDirectAttackFallback()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!IsInputBlocked())
            {
                EnsureCursorLocked();
                TryPerformAttack();
            }
        }
    }

    private void HandleSecondaryInput()
    {
        if (IsInputBlocked()) return;

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            EnsureCursorLocked();
            Debug.Log($"<color=#00FFFF>[PlayerHand] Kliknięto PPM! Przedmiot w dłoni: {(currentItem != null ? currentItem.itemName : "Brak")}, isAttacking: {isAttacking}</color>");
            if (currentItem != null)
            {
                currentItem.AltUse(GetComponentInParent<PlayerController>(), this);
            }
        }
    }

    private bool isUpdatingVisuals = false;

    public void UpdateHandVisuals()
    {
        if (isUpdatingVisuals) return;
        isUpdatingVisuals = true;

        try
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (handRenderingSetup == null) handRenderingSetup = GetComponentInParent<HandRenderingSetup>() ?? GetComponent<HandRenderingSetup>();
            if (playerController == null) playerController = GetComponentInParent<PlayerController>();
            if (spriteRenderer == null) return;
            if (Inventory.instance == null || isAttacking) return;

            Item item = Inventory.instance.GetActiveItem();
            EquippableItem previousItem = currentItem;

            if (item is EquippableItem equippable)
            {
                if (previousItem != equippable)
                {
                    if (previousItem != null)
                    {
                        previousItem.OnUnequipped(playerController, this);
                    }
                    equippable.OnEquipped(playerController, this);
                }

                currentItem = equippable;
                if (equippable.heldSprite != null)
                {
                    spriteRenderer.sprite = equippable.heldSprite;
                }
                else if (equippable.icon != null)
                {
                    spriteRenderer.sprite = equippable.icon;
                }
                else
                {
                    spriteRenderer.sprite = null;
                }
            }
            else
            {
                if (previousItem != null)
                {
                    previousItem.OnUnequipped(playerController, this);
                }

                currentItem = null;
                if (idleSprite != null)
                {
                    spriteRenderer.sprite = idleSprite;
                }
                else
                {
                    spriteRenderer.sprite = null;
                }
            }

            // Apply item position offset directly
            Vector3 spriteOffset = (handRenderingSetup != null) ? handRenderingSetup.CurrentSpritePositionOffset : Vector3.zero;
            Vector3 totalOffset = ((currentItem != null) ? currentItem.handPositionOffset : Vector3.zero) + livePositionOffset + spriteOffset;
            transform.localPosition = defaultLocalPos + totalOffset;
        }
        finally
        {
            isUpdatingVisuals = false;
        }
    }

    private void HandleBobbing()
    {
        if (isAttacking) return;
        
        Vector3 spriteOffset = (handRenderingSetup != null) ? handRenderingSetup.CurrentSpritePositionOffset : Vector3.zero;
        Vector3 totalOffset = ((currentItem != null) ? currentItem.handPositionOffset : Vector3.zero) + livePositionOffset + spriteOffset;
        Vector3 basePos = defaultLocalPos + totalOffset;

        if (characterController != null && characterController.velocity.magnitude > 0.1f && characterController.isGrounded)
        {
             timer += Time.deltaTime * bobSpeed;
             float newY = basePos.y + Mathf.Sin(timer) * bobAmount;
             transform.localPosition = new Vector3(basePos.x, newY, basePos.z);
        }
        else
        {
            timer = 0;
            transform.localPosition = basePos;
        }

        if (liveRotationOffset != Vector3.zero)
        {
            transform.localRotation = Quaternion.Euler(liveRotationOffset);
        }
    }

    [ContextMenu("Zapisz Live Offset Do Aktualnego Przedmiotu")]
    public void SaveLiveOffsetToCurrentItem()
    {
        if (currentItem != null)
        {
            currentItem.handPositionOffset += livePositionOffset;
            livePositionOffset = Vector3.zero;
            Debug.Log($"<color=#39FF14>[PlayerHand] Zapisano nową pozycję do {currentItem.name}: {currentItem.handPositionOffset}</color>");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(currentItem);
#endif
        }
    }

    public bool IsAttacking { get => isAttacking; set => isAttacking = value; }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (IsInputBlocked()) return;
        EnsureCursorLocked();
        TryPerformAttack();
    }

    private void TryPerformAttack()
    {
        float effectiveCooldown = (currentItem != null && currentItem.cooldown > 0f) ? currentItem.cooldown : attackCooldown;
        if (Time.time < lastAttackTime + effectiveCooldown || isAttacking) return;

        // Visual feedback for attack
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Attack Visuals
        if (currentItem != null && currentItem is WeaponItem weapon && weapon.attackSprite != null)
        {
            spriteRenderer.sprite = weapon.attackSprite;
        }
        else if (attackSprite != null && currentItem == null) // Default fist attack
        {
            spriteRenderer.sprite = attackSprite;
        }
        
        // Use Item Logic
        if (currentItem != null)
        {
            currentItem.Use(GetComponentInParent<PlayerController>(), this);
        }
        else
        {
             Debug.Log("Punching!");
             PerformRaycast(); // Default Punch
        }

        // Rotation Animation
        bool useRotation = false;
        float rotSpeed = 10f;

        if (currentItem != null && currentItem.useItemRotation)
        {
            useRotation = true;
            rotSpeed = currentItem.rotationSpeed;
        }

        if (useRotation && currentItem != null)
        {
            Quaternion animStart = Quaternion.identity;
            Quaternion animTarget = Quaternion.Euler(currentItem.rotationEuler);
            Quaternion initialLocalRot = transform.localRotation; // Fallback baseline

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * rotSpeed;
                Quaternion currentAnimRot = Quaternion.Slerp(animStart, animTarget, t);
                
                if (handRenderingSetup != null) 
                {
                    handRenderingSetup.animatedRotation = currentAnimRot;
                }
                else 
                {
                    transform.localRotation = initialLocalRot * currentAnimRot;
                }
                yield return null;
            }

            float returnSpeed = rotSpeed * (currentItem != null ? Mathf.Max(0.1f, currentItem.returnRotationSpeedMultiplier) : 1f);
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * returnSpeed;
                Quaternion currentAnimRot = Quaternion.Slerp(animTarget, animStart, t);
                 
                if (handRenderingSetup != null) 
                {
                    handRenderingSetup.animatedRotation = currentAnimRot;
                }
                else 
                {
                    transform.localRotation = initialLocalRot * currentAnimRot;
                }
                yield return null;
            }
            
            // Final Reset
            if (handRenderingSetup != null) handRenderingSetup.animatedRotation = Quaternion.identity;
            else transform.localRotation = initialLocalRot;
        }
        else
        {
             yield return new WaitForSeconds(attackDuration);
        }

        // Reset Visuals
        isAttacking = false;
        UpdateHandVisuals();
    }

    /// <summary>
    /// Umożliwia uruchomienie własnej sekwencji ataku (np. ciężki zamach z PPM).
    /// </summary>
    public void TriggerCustomAttack(IEnumerator routine, float cooldownDuration)
    {
        if (isAttacking) return;
        lastAttackTime = Time.time + (cooldownDuration - attackCooldown);
        StartCoroutine(routine);
    }

    /// <summary>
    /// Animacja potężnego zamachu (np. uderzenie z góry dla Młotka do Kotletów).
    /// </summary>
    public IEnumerator HeavySwingRoutine(Vector3 swingEuler, float swingSpeed, System.Action onApex)
    {
        isAttacking = true;

        if (currentItem != null && currentItem is WeaponItem weapon && weapon.attackSprite != null)
        {
            spriteRenderer.sprite = weapon.attackSprite;
        }

        Quaternion animStart = Quaternion.identity;
        Quaternion animTarget = Quaternion.Euler(swingEuler);
        Quaternion initialLocalRot = transform.localRotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * swingSpeed;
            Quaternion currentAnimRot = Quaternion.Slerp(animStart, animTarget, t);
            if (handRenderingSetup != null) handRenderingSetup.animatedRotation = currentAnimRot;
            else transform.localRotation = initialLocalRot * currentAnimRot;
            yield return null;
        }

        onApex?.Invoke();

        float returnMult = currentItem != null ? Mathf.Max(0.1f, currentItem.returnRotationSpeedMultiplier) : 0.75f;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * (swingSpeed * returnMult);
            Quaternion currentAnimRot = Quaternion.Slerp(animTarget, animStart, t);
            if (handRenderingSetup != null) handRenderingSetup.animatedRotation = currentAnimRot;
            else transform.localRotation = initialLocalRot * currentAnimRot;
            yield return null;
        }

        if (handRenderingSetup != null) handRenderingSetup.animatedRotation = Quaternion.identity;
        else transform.localRotation = initialLocalRot;

        isAttacking = false;
        UpdateHandVisuals();
    }

    /// <summary>
    /// Animacja pchnięcia w przód (dla Włóczni).
    /// </summary>
    public IEnumerator SpearThrustRoutine(Vector3 thrustOffset, float thrustSpeed, System.Action onPeak)
    {
        isAttacking = true;

        if (currentItem != null && currentItem is WeaponItem weapon && weapon.attackSprite != null)
        {
            spriteRenderer.sprite = weapon.attackSprite;
        }

        Vector3 basePos = transform.localPosition;
        Vector3 targetPos = basePos + thrustOffset;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * thrustSpeed;
            transform.localPosition = Vector3.Lerp(basePos, targetPos, t);
            yield return null;
        }

        onPeak?.Invoke();

        float returnMult = currentItem != null ? Mathf.Max(0.1f, currentItem.returnRotationSpeedMultiplier) : 1f;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * (thrustSpeed * returnMult);
            transform.localPosition = Vector3.Lerp(targetPos, basePos, t);
            yield return null;
        }

        transform.localPosition = basePos;
        isAttacking = false;
        UpdateHandVisuals();
    }

    /// <summary>
    /// Wysuwa włócznię do przodu w pozycję bojową na czas trwania szarży.
    /// </summary>
    public IEnumerator SpearChargeStanceRoutine(Vector3 thrustOffset, float thrustSpeed)
    {
        isAttacking = true;

        if (currentItem != null && currentItem is WeaponItem weapon && weapon.attackSprite != null)
        {
            spriteRenderer.sprite = weapon.attackSprite;
        }

        Vector3 basePos = transform.localPosition;
        Vector3 targetPos = basePos + thrustOffset;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * thrustSpeed;
            transform.localPosition = Vector3.Lerp(basePos, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos;
    }

    /// <summary>
    /// Cofa wysuniętą włócznię po zakończeniu szarży z powrotem do pozycji spoczynkowej.
    /// </summary>
    public IEnumerator SpearReleaseChargeRoutine(Vector3 thrustOffset, float returnSpeed)
    {
        Vector3 targetPos = transform.localPosition;
        Vector3 basePos = targetPos - thrustOffset;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.localPosition = Vector3.Lerp(targetPos, basePos, t);
            yield return null;
        }

        transform.localPosition = basePos;
        isAttacking = false;
        UpdateHandVisuals();
    }

    /// <summary>
    /// Dynamiczna animacja cięcia sierpem po łuku: zamach od prawej/tyłu, cięcie przez środek w przód i w lewo.
    /// </summary>
    public IEnumerator SickleSlashRoutine(Vector3 prepOffset, Vector3 prepEuler, Vector3 slashOffset, Vector3 slashEuler, float slashSpeed, System.Action onApex)
    {
        isAttacking = true;

        Vector3 basePos = transform.localPosition;
        Quaternion baseRot = transform.localRotation;

        Vector3 prepPos = basePos + prepOffset;
        Quaternion prepRot = baseRot * Quaternion.Euler(prepEuler);

        Vector3 strikePos = basePos + slashOffset;
        Quaternion strikeRot = baseRot * Quaternion.Euler(slashEuler);

        // KROK 1: Szybki zamach od prawej (anticipation)
        float prepTime = 0.08f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / prepTime;
            transform.localPosition = Vector3.Lerp(basePos, prepPos, t);
            if (handRenderingSetup != null) handRenderingSetup.animatedRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(prepEuler), t);
            else transform.localRotation = Quaternion.Slerp(baseRot, prepRot, t);
            yield return null;
        }

        // KROK 2: Błyskawiczne, zamaszyste cięcie po łuku w przód i w lewo
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * slashSpeed;
            transform.localPosition = Vector3.Lerp(prepPos, strikePos, t);
            if (handRenderingSetup != null) handRenderingSetup.animatedRotation = Quaternion.Slerp(Quaternion.Euler(prepEuler), Quaternion.Euler(slashEuler), t);
            else transform.localRotation = Quaternion.Slerp(prepRot, strikeRot, t);
            yield return null;
        }

        // Punkt trafienia (apex)
        onApex?.Invoke();

        // KROK 3: Płynny powrót z lewej strony do pozycji neutralnej
        float returnSpeed = slashSpeed * 0.75f;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.localPosition = Vector3.Lerp(strikePos, basePos, t);
            if (handRenderingSetup != null) handRenderingSetup.animatedRotation = Quaternion.Slerp(Quaternion.Euler(slashEuler), Quaternion.identity, t);
            else transform.localRotation = Quaternion.Slerp(strikeRot, baseRot, t);
            yield return null;
        }

        transform.localPosition = basePos;
        if (handRenderingSetup != null) handRenderingSetup.animatedRotation = Quaternion.identity;
        else transform.localRotation = baseRot;

        isAttacking = false;
        UpdateHandVisuals();
    }

    public void PerformRaycast(int customBaseDamage = -1, System.Action<GameObject, IDamageable, RaycastHit> onHitCallback = null, float customRange = -1f)
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Use safe default for layers if nothing selected
        int layerMask = hitLayers.value == 0 ? ~0 : hitLayers.value;
        
        // range depends on item? For now usage defaults to this method for punches/weapons
        float range = customRange > 0f ? customRange : attackRange;
        if (customRange <= 0f && currentItem != null && currentItem is WeaponItem weapon)
        {
            range = weapon.range;
        }

        if (Physics.Raycast(ray, out hit, range, layerMask))
        {
            Debug.Log($"Hit: {hit.collider.name}");
            
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hit.collider.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                int baseDamage = customBaseDamage > 0 ? customBaseDamage : 10; // Default punch damage
                WeaponItem currentWeapon = currentItem as WeaponItem;
                int upgradeBonus = 0;
                int weaponLevel = 0;

                if (customBaseDamage <= 0 && currentWeapon != null)
                {
                    if (Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null)
                    {
                        upgradeBonus = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetBonusDamage(currentWeapon);
                        weaponLevel = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetUpgradeLevel(currentWeapon);
                    }
                    baseDamage = currentWeapon.damage + upgradeBonus;
                }

                PlayerStatsUpgrade statsUpgrade = GetComponentInParent<PlayerStatsUpgrade>();
                if (statsUpgrade == null) statsUpgrade = FindFirstObjectByType<PlayerStatsUpgrade>();
                
                int damage = statsUpgrade != null ? statsUpgrade.CalculateFinalDamage(baseDamage) : (playerStats != null ? playerStats.GetDamage(baseDamage) : baseDamage);

                // Bonus Mistrzowski na poziomie +6 (+15% szansy na trafienie krytyczne)
                if (currentWeapon != null && Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance != null && Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.HasMasterworkBonus(currentWeapon))
                {
                    float extraCrit = Praktyki.KowadloUlepszen.WeaponUpgradeManager.Instance.GetMasterworkCritBonus(currentWeapon);
                    if (Random.value < extraCrit)
                    {
                        damage = Mathf.RoundToInt(damage * 1.5f);
                        Debug.Log($"<color=#FFD700>[PlayerHand] MISTRZOWSKIE TRAFIENIE KRYTYCZNE (+6)! Zwiększono obrażenia do {damage}</color>");
                    }
                }
                
                Debug.Log($"<color=#39FF14>[PlayerHand]</color> Atak bronią: {currentItem?.itemName ?? "Pięść"} (Poziom: +{weaponLevel}) | Bazowy Damage broni: {baseDamage} (+{upgradeBonus} z ulepszeń) | Zadane obrażenia (z siłą gracza): {damage}");
                
                damageable.TakeDamage(damage);

                if (onHitCallback != null)
                {
                    onHitCallback(hit.collider.gameObject, damageable, hit);
                }
                else if (currentWeapon != null && hit.collider != null && hit.collider.gameObject.activeInHierarchy)
                {
                    currentWeapon.OnHit(hit.collider.gameObject, damageable, hit);
                }
            }
            
            // Also invoke ICollidable for physics interactions (like pushing) just in case
            ICollidable collidable = hit.collider.GetComponent<ICollidable>();
            if (collidable != null)
            {
                collidable.OnCollide(gameObject);
            }
        }
    }

    // Optional: Draw Gizmos to see range
    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * attackRange);
        }
    }
}
