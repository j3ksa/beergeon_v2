using System.Collections;
using UnityEngine;

namespace Praktyki.StaryHmielnik
{
    /// <summary>
    /// Kontroler zachowania Starego Chmielnika jako przeciwnika dystansowego.
    /// Zamiast biec na oślep w zwarciu, utrzymuje bezpieczny dystans (kiting),
    /// obraca się w stronę gracza i miota szyszkami chmielu w synchronizacji
    /// z animacją zamachu ręką (klatka 0.45s).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class StaryHmielnikController : MonoBehaviour
    {
        [Header("Konfiguracja Ruchu i Zasięgu")]
        [Tooltip("Prędkość poruszania się (stary chmielnik jest nieco wolniejszy i metodyczny)")]
        [SerializeField] private float moveSpeed = 2.5f;

        [Tooltip("Zasięg wykrywania gracza")]
        [SerializeField] private float detectionRange = 14f;

        [Tooltip("Maksymalny zasięg, z którego chmielnik rzuca szyszką")]
        [SerializeField] private float attackRange = 11f;

        [Tooltip("Preferowany dystans od gracza (jeśli gracz jest bliżej, chmielnik stoi lub cofa się)")]
        [SerializeField] private float preferredDistance = 6.5f;

        [Tooltip("Dystans wycofania (jeśli gracz podejdzie zbyt blisko, chmielnik cofa się)")]
        [SerializeField] private float retreatDistance = 3.0f;

        [Header("Konfiguracja Ataku Dystansowego")]
        [Tooltip("Prefab rzucanej szyszki chmielowej")]
        [SerializeField] private HopConeProjectile projectilePrefab;

        [Tooltip("Prędkość wylotowa szyszki (wyższa prędkość = trudniejszy unik dla gracza)")]
        [SerializeField] private float projectileSpeed = 19f;

        [Tooltip("Predykcja ruchu gracza / rzut na wyprzedzenie (0 = rzut w aktualną pozycję, 0.75 = rzut na wyprzedzenie biegu gracza)")]
        [Range(0f, 1.2f)]
        [SerializeField] private float leadTargetMultiplier = 0.75f;

        [Tooltip("Obrażenia zadawane przez szyszkę")]
        [SerializeField] private int projectileDamage = 10;

        [Tooltip("Czas odnowienia ataku (co ile sekund chmielnik rzuca)")]
        [SerializeField] private float attackCooldown = 2.2f;

        [Tooltip("Opóźnienie wystrzału w animacji (punkt wyrzutu szyszki ręką, klatka ~0.45s)")]
        [SerializeField] private float throwDelay = 0.45f;

        [Tooltip("Czas trwania całej animacji ataku")]
        [SerializeField] private float attackAnimDuration = 1.0f;

        [Tooltip("Punkt wylotu pocisku. Jeśli pusty, pocisk wylatuje ze środka klatki piersiowej zgodnie z spawnOffset")]
        [SerializeField] private Transform projectileSpawnPoint;

        [Tooltip("Offset spawnu pocisku ze środka chmielnika (X: środek, Y: wysokość klatki piersiowej, Z: w przód)")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, -0.05f, 0.6f);

        [Tooltip("Wysokość punktu celowania (offset względem Main Camera gracza, 0 = prosto w kamerę)")]
        [SerializeField] private float aimHeightOffset = 0f;

        [Tooltip("Łuk lotu szyszki w górę (0 = prosto w kamerę, 0.1-0.2 = lekki łuk balistyczny)")]
        [Range(0f, 0.5f)]
        [SerializeField] private float upwardArc = 0.05f;

        [Header("Referencje")]
        [SerializeField] private Transform target;
        [SerializeField] private Animator animator;

        private CharacterController characterController;
        private float gravity = -9.81f;
        private Vector3 velocity;
        private bool isAttacking = false;
        private float lastAttackTime = -999f;
        private Vector3 prevTargetPos;
        private Vector3 targetVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogWarning("[StaryHmielnikController] Brak komponentu Animator w dzieciach!");
            }
        }

        private void Start()
        {
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
            // Grawitacja
            if (characterController.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            velocity.y += gravity * Time.deltaTime;

            if (target == null)
            {
                characterController.Move(Vector3.up * velocity.y * Time.deltaTime);
                return;
            }

            // Śledzenie prędkości gracza do rzutu na wyprzedzenie
            CharacterController targetCC = target.GetComponent<CharacterController>();
            if (targetCC != null && targetCC.velocity.sqrMagnitude > 0.01f)
            {
                targetVelocity = Vector3.Lerp(targetVelocity, targetCC.velocity, Time.deltaTime * 12f);
            }
            else
            {
                Vector3 delta = (target.position - prevTargetPos) / Mathf.Max(Time.deltaTime, 0.0001f);
                targetVelocity = Vector3.Lerp(targetVelocity, delta, Time.deltaTime * 12f);
            }
            prevTargetPos = target.position;

            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            Vector3 moveDirection = Vector3.zero;

            // Logika wykrywania i pozycjonowania dystansowego
            if (distanceToTarget <= detectionRange)
            {
                // Zawsze zwracaj się w stronę celu
                FaceTarget(target.position);

                if (!isAttacking)
                {
                    Vector3 directionToTarget = (target.position - transform.position).normalized;
                    directionToTarget.y = 0;

                    if (distanceToTarget > preferredDistance)
                    {
                        // Zbliżaj się do optymalnego dystansu
                        moveDirection = directionToTarget * moveSpeed;
                    }
                    else if (distanceToTarget < retreatDistance)
                    {
                        // Gracz podszedł za blisko - powoli cofaj się (kiting)
                        moveDirection = -directionToTarget * (moveSpeed * 0.75f);
                    }

                    // Próba ataku dystansowego
                    if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                    {
                        Attack();
                    }
                }
            }

            // Ruch CharacterController
            characterController.Move((moveDirection + Vector3.up * velocity.y) * Time.deltaTime);

            // Animacje chodu / bezruchu
            HandleAnimations(moveDirection);
        }

        private void FaceTarget(Vector3 targetPos)
        {
            Vector3 lookDirection = targetPos - transform.position;
            lookDirection.y = 0;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        private void Attack()
        {
            lastAttackTime = Time.time;
            isAttacking = true;

            if (animator != null)
            {
                animator.Play("atack", 0, 0f);
                animator.speed = 1f;
            }

            // Rzut szyszką po throwDelay
            StartCoroutine(ThrowHopRoutine());

            // Reset stanu ataku po zakończeniu animacji
            StartCoroutine(ResetAttackRoutine(attackAnimDuration));
        }

        private IEnumerator ThrowHopRoutine()
        {
            yield return new WaitForSeconds(throwDelay);

            if (projectilePrefab != null && target != null)
            {
                Vector3 spawnPos = projectileSpawnPoint != null
                    ? projectileSpawnPoint.position
                    : transform.TransformPoint(spawnOffset);

                // Celujemy precyzyjnie w pozycję Main Camera gracza (wysokość oczu gracza)
                Vector3 aimTarget;
                Camera playerCam = target.GetComponentInChildren<Camera>();
                if (playerCam == null) playerCam = Camera.main;

                if (playerCam != null)
                {
                    aimTarget = playerCam.transform.position;
                }
                else
                {
                    aimTarget = target.position + Vector3.up * 0.6f;
                }

                // Dodatkowy offset wysokości (domyślnie 0, czyli dokładnie środek kamery)
                aimTarget.y += aimHeightOffset;

                // Predykcja ruchu gracza (rzut na wyprzedzenie biegu)
                float initialDistance = Vector3.Distance(spawnPos, aimTarget);
                float estimatedFlightTime = initialDistance / Mathf.Max(projectileSpeed, 1f);
                if (leadTargetMultiplier > 0f)
                {
                    Vector3 leadOffset = targetVelocity * (estimatedFlightTime * leadTargetMultiplier);
                    leadOffset.y = 0f; // ignoruj ruch pionowy, wyprzedzaj bieg w poziomie
                    aimTarget += leadOffset;
                }

                Vector3 toTarget = aimTarget - spawnPos;
                Vector3 throwDirection = toTarget.normalized;

                // Dodanie łuku balistycznego w górę
                if (upwardArc > 0f)
                {
                    throwDirection += Vector3.up * upwardArc;
                    throwDirection.Normalize();
                }

                HopConeProjectile projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(throwDirection));
                projectile.Initialize(throwDirection, projectileSpeed, projectileDamage, gameObject);
            }
            else if (projectilePrefab == null)
            {
                Debug.LogWarning("[StaryHmielnikController] Brak przypisanego prefabu 'projectilePrefab' (HopConeProjectile)!");
            }
        }

        private IEnumerator ResetAttackRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            isAttacking = false;
        }

        private void HandleAnimations(Vector3 moveDirection)
        {
            if (animator == null) return;
            if (isAttacking) return;

            bool isMoving = moveDirection.magnitude > 0.1f;

            if (isMoving)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName("HmielnikWalk") || animator.speed == 0)
                {
                    animator.Play("HmielnikWalk");
                    animator.speed = 1f;
                }
            }
            else
            {
                if (animator.speed != 0)
                {
                    animator.Play("HmielnikWalk", 0, 0f);
                    animator.speed = 0f;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Rysowanie sfer zasięgów w Scene View dla łatwej konfiguracji
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, preferredDistance);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, retreatDistance);

            // Wizualizacja punktu wylotu szyszki (mała błękitna kulka)
            Vector3 spawnPreview = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.TransformPoint(spawnOffset);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(spawnPreview, 0.08f);
        }
    }
}
