using UnityEngine;

public class ExpItem : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int xpValue = 10;

    [Header("Magnet Effect")]
    [SerializeField] private float magnetRadius = 5f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float acceleration = 8f;

    private Transform playerTransform;
    private bool isFollowingPlayer = false;

    public void SetXpValue(int value)
    {
        xpValue = value;
    }

    private void Start()
    {
        // Try to find the player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            // Fallback: search for PlayerStats in scene
            PlayerStats stats = Object.FindFirstObjectByType<PlayerStats>();
            if (stats != null)
            {
                playerTransform = stats.transform;
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Activate magnet mode if player is close enough
        if (!isFollowingPlayer && distance <= magnetRadius)
        {
            isFollowingPlayer = true;
        }

        if (isFollowingPlayer)
        {
            // Smoothly fly towards player
            speed += acceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the Player
        if (other.CompareTag("Player") || other.GetComponent<PlayerStats>() != null)
        {
            Collect(other.gameObject);
        }
    }

    private void Collect(GameObject player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null)
        {
            stats = Object.FindFirstObjectByType<PlayerStats>();
        }

        if (stats != null)
        {
            stats.GainExperience(xpValue);
            Destroy(gameObject);
        }
    }
}
