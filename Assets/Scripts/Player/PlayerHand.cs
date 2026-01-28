using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Inventory Integration")]
    [SerializeField] private PlayerController playerController;

    private float defaultYPos;
    private float timer = 0;
    private bool isAttacking = false;
    private float lastAttackTime = 0;
    
    // Store current equipped info
    private EquippableItem currentItem;

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerCamera == null) playerCamera =  Camera.main;
        
        defaultYPos = transform.localPosition.y;
        
        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }

        // Try to find stats if not assigned
        if (playerStats == null)
        {
            playerStats = GetComponentInParent<PlayerStats>();
        }
        
        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();
        }

        if (playerController == null) playerController = GetComponentInParent<PlayerController>();

        // Init initial state
        if (Inventory.instance != null)
        {
            Inventory.instance.OnActiveSlotChanged.AddListener(UpdateHandVisuals);
            UpdateHandVisuals(); // Set initial
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

        if (Inventory.instance != null)
        {
            Inventory.instance.OnActiveSlotChanged.RemoveListener(UpdateHandVisuals);
        }
    }

    private void Update()
    {
        HandleBobbing();
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
        // Check cooldown from item or default
        float cooldown = (currentItem != null) ? currentItem.cooldown : attackCooldown;

        if (Time.time < lastAttackTime + cooldown || isAttacking) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Visual change
        bool resetSprite = false;

        if (currentItem != null)
        {
            // If item has a specific attack sprite, use it. 
            // If internal weapon logic handles it, skipping sprite change here might be needed, 
            // but for now we follow the plan: WeaponItem uses heldSprite primarily, unless attackSprite is set.
             if (currentItem is WeaponItem weapon && weapon.attackSprite != null)
             {
                 spriteRenderer.sprite = weapon.attackSprite;
                 resetSprite = true;
             }
             // For Potions, we might want a drinking animation/sprite later?
        }
        else
        {
             // Default fist
             if (attackSprite != null)
             {
                 spriteRenderer.sprite = attackSprite;
                 resetSprite = true;
             }
        }
        
        

        // Logic
        if (currentItem != null)
        {
            currentItem.Use(playerController, this);
        }
        else
        {
             // Default Punch
             PerformRaycast();
        }

        // Use item duration if applicable, else default
        // Weapon items might define a swing time separate from cooldown
        yield return new WaitForSeconds(attackDuration);

        // Reset visual
        if (resetSprite)
        {
            UpdateHandVisuals();
        }
        
        isAttacking = false;
    }

    public void UpdateHandVisuals()
    {
        if (Inventory.instance == null) return;

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
                // Fallback if item has no sprite??
                spriteRenderer.sprite = null; 
            }
        }
        else
        {
            currentItem = null;
            // Default Open Hand
            if (idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
        }
    }

    private void PerformRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Use safe default for layers if nothing selected
        int layerMask = hitLayers.value == 0 ? ~0 : hitLayers.value;

        if (Physics.Raycast(ray, out hit, attackRange, layerMask))
        {
            
            
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                int damage = playerStats != null ? playerStats.GetDamage() : 1;
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
