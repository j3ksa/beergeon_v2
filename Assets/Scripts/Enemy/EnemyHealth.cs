using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public UnityEvent OnDeath;
    public UnityEvent<int> OnDamageTaken;
    private EnemyAudioController enemyAudio;

    void Start()
    {
        enemyAudio = GetComponent<EnemyAudioController>();
    }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth > 0 && enemyAudio != null)
        {
            enemyAudio.PlayHitSound();
        }

        OnDamageTaken?.Invoke(damage);

        // Simple Hit Flash Effect
        StartCoroutine(FlashColor());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 1. DŸwiêk musi byæ PIERWSZY
        if (enemyAudio != null)
        {
            enemyAudio.PlayDeathSound();
        }

        OnDeath?.Invoke();

        // 2. Zamiast natychmiastowego Destroy(gameObject), dajemy mu 0.1s zw³oki
        // To wystarczy, by FMOD odebra³ sygna³ PlayOneShot
        Destroy(gameObject, 0.1f);

        // Opcjonalnie: wy³¹czamy grafiki, ¿eby nie by³o widaæ "¿ywego" trupa przez te 0.1s
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null) renderer.enabled = false;
    }

    private System.Collections.IEnumerator FlashColor()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }
}
