using UnityEngine;

namespace Praktyki.StaryHmielnik
{
    /// <summary>
    /// Pocisk rzucany przez Starego Chmielnika (szyszka chmielowa).
    /// Posiada balistyczny tor lotu (parabola z grawitacją), rotację w locie (koziołkowanie),
    /// zadaje obrażenia graczowi oraz nakłada efekt spowolnienia.
    /// </summary>
    public class HopConeProjectile : MonoBehaviour
    {
        [Header("Konfiguracja Lotu")]
        [Tooltip("Początkowa prędkość lotu szyszki")]
        [SerializeField] private float speed = 14f;

        [Tooltip("Mnożnik grawitacji (0 = idealna linia prosta, 0.4-0.8 = naturalna lekka parabola)")]
        [SerializeField] private float gravityMultiplier = 0.5f;

        [Tooltip("Prędkość koziołkowania/obrotu szyszki w locie (stopnie na sekundę)")]
        [SerializeField] private float tumbleSpeed = 360f;

        [Tooltip("Maksymalny czas życia pocisku przed automatycznym usunięciem")]
        [SerializeField] private float lifetime = 8f;

        [Header("Obrażenia i Efekty")]
        [Tooltip("Liczba punktów obrażeń zadawanych przy trafieniu")]
        [SerializeField] private int damage = 10;

        [Tooltip("Czas trwania efektu spowolnienia po trafieniu (w sekundach)")]
        [SerializeField] private float slowDuration = 2.5f;

        [Tooltip("Mnożnik prędkości gracza pod wpływem spowolnienia (0.5 = 50% prędkości)")]
        [SerializeField] private float slowMultiplier = 0.5f;

        [Tooltip("Promień wybuchu zarodników pleśni przy uderzeniu o podłoże/ścianę (np. 1.6m)")]
        [SerializeField] private float splashRadius = 1.6f;

        [Header("Zatrucie (Poison)")]
        [Tooltip("Szansa na zatrucie gracza przy trafieniu (0.15 = 15% szansy)")]
        [Range(0f, 1f)]
        [SerializeField] private float poisonChance = 0.15f;

        [Tooltip("Czas trwania zatrucia w sekundach")]
        [SerializeField] private float poisonDuration = 5f;

        [Tooltip("Obrażenia od zatrucia na sekundę (10 HP/s)")]
        [SerializeField] private int poisonDamagePerSecond = 10;

        [Tooltip("Warstwy, w które pocisk może trafić (domyślnie gracz i ściany/ziemia)")]
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Referencje Wizualne i Efekty")]
        [Tooltip("Obiekt wizualny (dziecko), który obraca się w locie")]
        [SerializeField] private Transform visualRoot;

        [Tooltip("Opcjonalny prefab efektu cząsteczkowego przy rozbiciu szyszki")]
        [SerializeField] private GameObject impactEffectPrefab;

        [Tooltip("Opcjonalny dźwięk uderzenia szyszki")]
        [SerializeField] private AudioClip impactSound;

        private Vector3 velocity;
        private bool hasHit = false;
        private GameObject shooter;
        private float spawnTime;

        /// <summary>
        /// Inicjalizacja kierunku i parametrów wystrzelonej szyszki.
        /// </summary>
        public void Initialize(Vector3 direction, float customSpeed, int customDamage, GameObject shooterObject)
        {
            if (customSpeed > 0) speed = customSpeed;
            if (customDamage > 0) damage = customDamage;

            shooter = shooterObject;
            velocity = direction.normalized * speed;
            spawnTime = Time.time;

            if (direction != Vector3.zero)
            {
                transform.forward = direction.normalized;
            }
        }

        private void Start()
        {
            if (spawnTime == 0) spawnTime = Time.time;
            if (velocity == Vector3.zero) velocity = transform.forward * speed;

            // Ignoruj kolizję ze strzelcem (chmielnikiem)
            if (shooter != null)
            {
                Collider[] myColliders = GetComponentsInChildren<Collider>();
                Collider[] shooterColliders = shooter.GetComponentsInChildren<Collider>();
                foreach (var myCol in myColliders)
                {
                    foreach (var sCol in shooterColliders)
                    {
                        Physics.IgnoreCollision(myCol, sCol, true);
                    }
                }
            }
        }

        private void Update()
        {
            if (hasHit) return;

            // Sprawdzenie czasu życia
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Grawitacja (parabola lotu)
            if (gravityMultiplier > 0)
            {
                velocity += Physics.gravity * gravityMultiplier * Time.deltaTime;
            }

            // Koziołkowanie / rotacja wizualna szyszki w locie
            if (visualRoot != null)
            {
                visualRoot.Rotate(Vector3.forward, tumbleSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                transform.Rotate(Vector3.forward, tumbleSpeed * Time.deltaTime, Space.Self);
            }

            // Ruch z raycastem (precyzyjna detekcja trafienia)
            Vector3 step = velocity * Time.deltaTime;
            float stepDistance = step.magnitude;
            Ray ray = new Ray(transform.position, velocity.normalized);

            if (Physics.Raycast(ray, out RaycastHit hit, stepDistance, hitLayers, QueryTriggerInteraction.Ignore))
            {
                // Ignoruj trafienie w samego siebie lub strzelca
                if (hit.collider.gameObject == shooter || hit.collider.transform.IsChildOf(shooter.transform))
                {
                    transform.position += step;
                    return;
                }

                OnHit(hit.collider, hit.point, hit.normal);
                return;
            }

            // Aktualizacja pozycji
            transform.position += step;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;
            if (shooter != null && (other.gameObject == shooter || other.transform.IsChildOf(shooter.transform)))
            {
                return;
            }

            OnHit(other, transform.position, -transform.forward);
        }

        private void OnHit(Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal)
        {
            hasHit = true;
            bool directHitPlayer = false;

            // 1. Bezpośrednie trafienie
            IDamageable damageable = hitCollider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            else
            {
                PlayerHealth playerHealth = hitCollider.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    directHitPlayer = true;
                }
            }

            PlayerController player = hitCollider.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                directHitPlayer = true;
                ApplyEffectsToPlayer(player);
            }

            // 2. Eksplozja zarodników pleśni w promieniu splashRadius (jeśli szyszka trafiła o podłoże/ścianę)
            if (!directHitPlayer && splashRadius > 0f)
            {
                Collider[] nearby = Physics.OverlapSphere(hitPoint, splashRadius, hitLayers, QueryTriggerInteraction.Ignore);
                foreach (var col in nearby)
                {
                    if (col == hitCollider || (shooter != null && (col.gameObject == shooter || col.transform.IsChildOf(shooter.transform))))
                        continue;

                    PlayerController nearbyPlayer = col.GetComponentInParent<PlayerController>();
                    if (nearbyPlayer != null)
                    {
                        PlayerHealth pHealth = nearbyPlayer.GetComponent<PlayerHealth>();
                        if (pHealth != null)
                        {
                            pHealth.TakeDamage(Mathf.RoundToInt(damage * 0.75f));
                        }
                        ApplyEffectsToPlayer(nearbyPlayer);
                        break;
                    }
                }
            }

            // 3. Efekty cząsteczkowe / dźwięk
            if (impactEffectPrefab != null)
            {
                Instantiate(impactEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            }

            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, hitPoint);
            }

            // Usunięcie szyszki
            Destroy(gameObject);
        }

        private void ApplyEffectsToPlayer(PlayerController targetPlayer)
        {
            if (targetPlayer == null) return;

            // 1. Spowolnienie + zielona lekka ramka na ekranie
            HopSlowDebuff slowDebuff = targetPlayer.GetComponent<HopSlowDebuff>();
            if (slowDebuff == null)
            {
                slowDebuff = targetPlayer.gameObject.AddComponent<HopSlowDebuff>();
            }
            slowDebuff.ApplySlow(slowMultiplier, slowDuration);

            // 2. Szansa na zatrucie (15%) -> 10 HP/s przez 5s + zielony pasek życia (jeśli gracz nie ma odporności z Mleka Starego)
            if (!Praktyki.MlekoStarego.MlekoStaregoBuff.IsImmune && Random.value <= poisonChance)
            {
                HopPoisonDebuff poisonDebuff = targetPlayer.GetComponent<HopPoisonDebuff>();
                if (poisonDebuff == null)
                {
                    poisonDebuff = targetPlayer.gameObject.AddComponent<HopPoisonDebuff>();
                }
                poisonDebuff.ApplyPoison(poisonDuration, poisonDamagePerSecond);
            }
        }
    }
}
