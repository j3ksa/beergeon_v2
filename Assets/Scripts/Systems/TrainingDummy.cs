using UnityEngine;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private int health = 100;
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        
        // Simple hit effect
        StartCoroutine(FlashRed());
        
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator FlashRed()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
        }
    }
}
