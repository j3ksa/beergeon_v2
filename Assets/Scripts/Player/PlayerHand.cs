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
    
    // Reference to HandRenderingSetup to control rotation without conflict
    private HandRenderingSetup handRenderingSetup;

    private float defaultYPos;
    private float timer = 0;
    private bool isAttacking = false;
    private float lastAttackTime = 0;

    // Current Item
    private EquippableItem currentItem;

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCamera == null) playerCamera =  Camera.main;
        
        // Try to find HandRenderingSetup
        handRenderingSetup = GetComponentInParent<HandRenderingSetup>();
        if (handRenderingSetup == null) handRenderingSetup = GetComponent<HandRenderingSetup>();
        // If still null, maybe on child? but usually on parent or self.
        
        defaultYPos = transform.localPosition.y;
        
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
    }

    private void Update()
    {
        HandleBobbing();
    }

    public void UpdateHandVisuals()
    {
        if (Inventory.instance == null || isAttacking) return;

        Item item = Inventory.instance.GetActiveItem();
        if (item is EquippableItem equippable)
        {
            currentItem = equippable;
            if (equippable.heldSprite != null)
            {
                spriteRenderer.sprite = equippable.heldSprite;
            }
            else
            {
                spriteRenderer.sprite = null; // Or keep idle? Usually held sprite overrides.
            }
        }
        else
        {
            currentItem = null;
            if (idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
        }
    }

    private void HandleBobbing()
    {
        if (isAttacking) return;
        
        if (characterController != null && characterController.velocity.magnitude > 0.1f && characterController.isGrounded)
        {
             timer += Time.deltaTime * bobSpeed;
             float newY = defaultYPos + Mathf.Sin(timer) * bobAmount;
             transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
        else
        {
            timer = 0;
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, defaultYPos, Time.deltaTime * bobSpeed), transform.localPosition.z);
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        // Block attack if game is paused (timeScale is 0) or cursor is unlocked (interacting with UI)
        if (Time.timeScale == 0f || Cursor.lockState != CursorLockMode.Locked) return;

        if (Time.time < lastAttackTime + attackCooldown || isAttacking) return;

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

            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * rotSpeed;
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

    public void PerformRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Use safe default for layers if nothing selected
        int layerMask = hitLayers.value == 0 ? ~0 : hitLayers.value;
        
        // range depends on item? For now usage defaults to this method for punches/weapons
        float range = attackRange;
        if (currentItem != null && currentItem is WeaponItem weapon)
        {
            range = weapon.range;
        }

        if (Physics.Raycast(ray, out hit, range, layerMask))
        {
            Debug.Log($"Hit: {hit.collider.name}");
            
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                int baseDamage = 10; // Default punch damage
                if (currentItem != null && currentItem is WeaponItem w)
                {
                    baseDamage = w.damage;
                }
                
                int damage = playerStats != null ? playerStats.GetDamage(baseDamage) : baseDamage;
                
                damageable.TakeDamage(damage);
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
