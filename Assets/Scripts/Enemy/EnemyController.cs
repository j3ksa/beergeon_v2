using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int damage = 10;
    
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private CharacterController characterController;
    private float gravity = -9.81f;
    private Vector3 velocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (animator == null) Debug.LogError("EnemyController: No Animator found in children!");
    }

    private void Start()
    {
        // Find player if target not set
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Ensure walk animation plays by default if using the Hmielnik controller
        if (animator != null)
        {
             // The default state in the controller is already Walk, so no action needed usually.
             // But valid to check if it's playing.
        }
    }

    private void Update()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Apply Gravity
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        Vector3 moveDirection = Vector3.zero;

        if (distanceToTarget <= detectionRange)
        {
            if (distanceToTarget > attackRange)
            {
                // Move towards player
                // We keep y = 0 to avoid tilting or moving up/down excessively in calculation
                Vector3 direction = (target.position - transform.position).normalized;
                direction.y = 0; 
                
                moveDirection = direction * moveSpeed;
                
                // Animation
                if (animator != null)
                {
                    // Assuming "HmielnikWalk" is the state name or trigger. 
                    // Based on the controller file, it seems like a default state or needs parameters?
                    // The provided controller has 'HmielnikWalk' as default state.
                    // If we need to toggle walk/idle, we might need a parameter.
                    // For now, if moving, it just plays the default walk animation.
                }
            }
            else
            {
                // Attack logic placeholder
                Attack();
            }
        }

        // Apply movement
        characterController.Move((moveDirection + Vector3.up * velocity.y) * Time.deltaTime);
    }

    private float lastAttackTime;
    [SerializeField] private float attackCooldown = 1.0f;

    private void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("Enemy Attacks!");
            
            // Try to damage player
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            else
            {
                // Fallback for PlayerHealth if it doesn't implement IDamageable directly yet (it assumes direct method call in some games)
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
            
            // Trigger attack animation if available
             if (animator != null)
            {
                animator.Play("atack", 0, 0f); // Using "atack" state from the provided controller file content
            }
        }
    }
}
