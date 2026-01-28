using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public UnityEvent OnDeath;
    public UnityEvent<int> OnDamageTaken;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        
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
        
        OnDeath?.Invoke();
        
        // Simple death handling: Destroy object
        // ideally we might play an animation or spawn a corpse
        Destroy(gameObject);
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
