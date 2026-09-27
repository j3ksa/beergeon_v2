using UnityEngine;

namespace Praktyki.OgnistaWoda
{
    /// <summary>
    /// Skrypt mikstury "Ognista Woda" w świecie gry (World Pickup).
    /// Posiada automatyczny Billboard (obraca się do kamery gracza), bez efektu magnesu (dokładnie jak Bycze Paliwo, Małpka i Mocny Gul).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class OgnistaWodaPotion : MonoBehaviour
    {
        [Header("Statystyki Buffa")]
        [Tooltip("Procent redukcji obrażeń (np. 0.25 = 25% mniejsze obrażenia)")]
        [SerializeField] [Range(0.05f, 0.95f)] private float reductionPercent = 0.25f;

        [Tooltip("Czas trwania buffa obrony w sekundach")]
        [SerializeField] private float duration = 10.0f;

        [Header("Efekty Wizualne i Dźwiękowe")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupSound;

        private bool isCollected = false;

        private void Start()
        {
            // Automatycznie dodajemy komponent Billboard, aby sprite obracał się do kamery gracza
            if (GetComponent<Billboard>() == null)
            {
                gameObject.AddComponent<Billboard>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerHealth>() != null || other.GetComponent<PlayerController>() != null)
            {
                Collect(other.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerHealth>() != null || other.GetComponent<PlayerController>() != null)
            {
                Collect(other.gameObject);
            }
        }

        private void Collect(GameObject player)
        {
            isCollected = true;

            // Nałożenie buffa redukcji obrażeń
            OgnistaWodaBuff.ApplyBuff(reductionPercent, duration);

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
