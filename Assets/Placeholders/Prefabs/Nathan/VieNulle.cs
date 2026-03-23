using UnityEngine;
using UnityEngine.UI;

public class VieNulle : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public Image healthbar;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (healthbar != null)
        {
            healthbar.fillAmount = currentHealth / maxHealth;
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
