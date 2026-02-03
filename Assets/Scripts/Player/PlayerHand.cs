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

    private float defaultYPos;
    private float timer = 0;
    private bool isAttacking = false;
    private float lastAttackTime = 0;

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
        
        // Try to find character controller for bobbing
        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();
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

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Visual change
        if (attackSprite != null)
        {
            spriteRenderer.sprite = attackSprite;
        }
        
        Debug.Log("Attacking!");

        // Logic
        PerformRaycast();

        yield return new WaitForSeconds(attackDuration);

        // Reset visual
        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
        
        isAttacking = false;
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
            Debug.Log($"Hit: {hit.collider.name}");
            
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
