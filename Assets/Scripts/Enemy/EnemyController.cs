using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyController : MonoBehaviour

{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackAnimDuration = 1.0f; // Duration of 'atack' animation
    
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private CharacterController characterController;
    private float gravity = -9.81f;
    private Vector3 velocity;
    private bool isAttacking = false;

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

        if (distanceToTarget <= detectionRange && !isAttacking)
        {
            if (distanceToTarget > attackRange)
            {
                // Move towards player
                Vector3 direction = (target.position - transform.position).normalized;
                direction.y = 0; 
                
                moveDirection = direction * moveSpeed;
            }
            else
            {
                Attack();
            }
        }

        // Apply movement
        characterController.Move((moveDirection + Vector3.up * velocity.y) * Time.deltaTime);

        // Animation Logic
        HandleAnimations(moveDirection);
    }

    private void HandleAnimations(Vector3 moveDirection)
    {
        if (animator == null) return;

        if (isAttacking)
        {
            // Animation logic for attacking is handled in Attack() and ResetAttack()
            return;
        }

        // Check if moving (ignoring vertical movement due to gravity)
        bool isMoving = moveDirection.magnitude > 0.1f;

        if (isMoving)
        {
            // Walking
            // Play "HmielnikWalk" if not already playing or if it was paused
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("HmielnikWalk") || animator.speed == 0)
            {
                animator.Play("HmielnikWalk");
                animator.speed = 1f;
            }
        }
        else
        {
            // Idle (Simulated using paused Walk frame 0)
            // Only force this if we are not already in this "state" to avoid jitter
            if (animator.speed != 0)
            {
                animator.Play("HmielnikWalk", 0, 0f);
                animator.speed = 0f;
            }
        }
    }

    private float lastAttackTime;
    [SerializeField] private float attackCooldown = 1.5f; // Increased slightly to match anim duration + buffer

    private void Attack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            
            // Damage Logic
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            else
            {
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
            
            // Animation Logic
            if (animator != null)
            {
                isAttacking = true;
                animator.Play("atack", 0, 0f);
                animator.speed = 1f;
                StartCoroutine(ResetAttackAfterDelay(attackAnimDuration));
            }
        }
    }

    private System.Collections.IEnumerator ResetAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        // The Update loop will pick up next frame and switch to Walk/Idle
    }
}
