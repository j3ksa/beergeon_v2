using UnityEngine;

namespace CurrencySystem
{
    public class CapItem : MonoBehaviour
    {
        [Header("Cap Settings")]
        [SerializeField] private int capValue = 1;

        [Header("Magnet Effect")]
        [SerializeField] private float magnetRadius = 5f;
        [SerializeField] private float speed = 2f;
        [SerializeField] private float acceleration = 8f;

        private Transform playerTransform;
        private bool isFollowingPlayer = false;

        public void SetCapValue(int value)
        {
            capValue = Mathf.Max(1, value);
        }

        private void Start()
        {
            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                PlayerCurrency currency = Object.FindFirstObjectByType<PlayerCurrency>();
                if (currency != null)
                {
                    playerTransform = currency.transform;
                }
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (!isFollowingPlayer && distance <= magnetRadius)
            {
                isFollowingPlayer = true;
            }

            if (isFollowingPlayer)
            {
                speed += acceleration * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerCurrency>() != null || other.GetComponent<PlayerStats>() != null)
            {
                Collect(other.gameObject);
            }
        }

        private void Collect(GameObject playerObj)
        {
            PlayerCurrency currency = playerObj.GetComponent<PlayerCurrency>();
            if (currency == null)
            {
                currency = PlayerCurrency.Instance ?? Object.FindFirstObjectByType<PlayerCurrency>();
            }

            if (currency != null)
            {
                currency.AddCaps(capValue);
            }
            else
            {
                Debug.LogWarning("[CapItem] Nie znaleziono PlayerCurrency na obiekcie gracza!");
            }

            Destroy(gameObject);
        }
    }
}
