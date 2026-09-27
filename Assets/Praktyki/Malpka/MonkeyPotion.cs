using UnityEngine;

namespace Praktyki.Malpka
{
    /// <summary>
    /// Skrypt mikstury "Małpka" zwiekszajacej obrazenia gracza.
    /// Umiesc ten skrypt na obiekcie mikstury (np. ze Sprite Rendererem i Colliderem zamienionym na Trigger).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MonkeyPotion : MonoBehaviour
    {
        [Header("Statystyki Buffa")]
        [Tooltip("Mnoznik obrazen (np. 1.5 oznacza +50% obrazen, 2.0 oznacza podwojne obrazenia)")]
        [SerializeField] private float damageMultiplier = 1.5f;

        [Tooltip("Czas trwania buffa obrazen w sekundach")]
        [SerializeField] private float buffDuration = 10.0f;

        [Header("Efekt Magnesu (Opcjonalny)")]
        [SerializeField] private bool useMagnetEffect = true;
        [SerializeField] private float magnetRadius = 4.0f;
        [SerializeField] private float flySpeed = 5.0f;

        [Header("Efekty Wizualne i Dzwiekowe")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        private Transform playerTransform;
        private bool isFollowingPlayer = false;
        private bool isCollected = false;

        private void Start()
        {
            // Automatycznie dodajemy komponent Billboard, aby sprite obracał się zawsze przodem do gracza/kamery (tak jak Mocny Gul)
            if (GetComponent<Billboard>() == null)
            {
                gameObject.AddComponent<Billboard>();
            }

            // Szukamy gracza w scenie
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                PlayerStats stats = Object.FindFirstObjectByType<PlayerStats>();
                if (stats != null)
                {
                    playerTransform = stats.transform;
                }
            }
        }

        private void Update()
        {
            if (!useMagnetEffect || isCollected || playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (!isFollowingPlayer && distance <= magnetRadius)
            {
                isFollowingPlayer = true;
            }

            if (isFollowingPlayer)
            {
                flySpeed += 5f * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, flySpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerStats>() != null)
            {
                Collect(other.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerStats>() != null)
            {
                Collect(other.gameObject);
            }
        }

        private void Collect(GameObject player)
        {
            isCollected = true;

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = Object.FindFirstObjectByType<PlayerStats>();
            }

            if (playerStats != null)
            {
                playerStats.ApplyDamageBuff(damageMultiplier, buffDuration);
                Debug.Log($"[MonkeyPotion] Gracz wypił Małpkę! Obrażenia x{damageMultiplier} na {buffDuration}s.");
            }

            // Opcjonalne efekty
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Destroy(gameObject);
        }
    }
}
