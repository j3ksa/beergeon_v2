using UnityEngine;

namespace Praktyki.ByczePaliwo
{
    /// <summary>
    /// Skrypt mikstury/energetyka "Bycze Paliwo" na mapie (World Pickup).
    /// Umieść ten skrypt na obiekcie mikstury w świecie gry ze Sprite Rendererem i Colliderem (Is Trigger = true).
    /// Po zebraniu przez gracza natychmiastowo uzupełnia staminę do 100%.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ByczePaliwoPotion : MonoBehaviour
    {
        [Header("Ustawienia Staminy")]
        [Tooltip("Czy ma natychmiast uzupełnić staminę na 100%")]
        [SerializeField] private bool instantFullStamina = true;
        [SerializeField] private float staminaAmount = 30f;

        [Header("Efekt Magnesu (Opcjonalny)")]
        [SerializeField] private bool useMagnetEffect = true;
        [SerializeField] private float magnetRadius = 4.0f;
        [SerializeField] private float flySpeed = 5.0f;

        [Header("Efekty Wizualne i Dźwiękowe")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        private Transform playerTransform;
        private bool isFollowingPlayer = false;
        private bool isCollected = false;

        private void Start()
        {
            // Automatycznie dodajemy komponent Billboard, aby sprite obracał się przodem do kamery gracza
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
                PlayerStamina stamina = Object.FindFirstObjectByType<PlayerStamina>();
                if (stamina != null)
                {
                    playerTransform = stamina.transform;
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

            if (other.CompareTag("Player") || other.GetComponent<PlayerStamina>() != null || other.GetComponent<PlayerController>() != null)
            {
                Collect(other.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerStamina>() != null || other.GetComponent<PlayerController>() != null)
            {
                Collect(other.gameObject);
            }
        }

        private void Collect(GameObject player)
        {
            isCollected = true;

            PlayerStamina playerStamina = player.GetComponent<PlayerStamina>();
            if (playerStamina == null)
            {
                playerStamina = Object.FindFirstObjectByType<PlayerStamina>();
            }

            if (playerStamina != null)
            {
                if (instantFullStamina)
                {
                    playerStamina.RestoreStaminaToFull();
                    Debug.Log("<color=#FFD700><b>[Bycze Paliwo]</b></color> Zebrano Bycze Paliwo! Stamina natychmiast uzupełniona na 100%.");
                }
                else
                {
                    playerStamina.RestoreStamina(staminaAmount);
                    Debug.Log($"<color=#FFD700><b>[Bycze Paliwo]</b></color> Zebrano Bycze Paliwo! Odnowiono {staminaAmount} staminy.");
                }
            }

            // Błysk energii na ekranie UI
            ByczePaliwoUI.TriggerFlash();

            // Efekty cząsteczkowe i dźwięk
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
