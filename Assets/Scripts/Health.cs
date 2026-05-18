using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Death")]
    public GameObject deathEffect;
    public AudioClip deathSound;

    private AudioSource audioSource;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // Prevent multiple death calls

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return; // Prevent multiple death calls
        isDead = true;

        Debug.Log($"{gameObject.name} died!");

        // Play death sound
        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

        // Spawn death effect
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, transform.rotation);

        // Notify the server that local player died
        if (ColyseusManager.Instance != null)
        {
            var room = ColyseusManager.Instance.GetRoom();
            if (room != null)
            {
                room.Send("playerKilled", new { playerId = room.SessionId });
                Debug.Log($"Sent playerKilled message for local player");
            }
        }

        // Return to lobby (the server will broadcast playerDied to all clients)
        if (ColyseusManager.Instance != null)
        {
            ColyseusManager.Instance.OnLocalPlayerDied();
        }

        // Destroy the gameobject
        Destroy(gameObject);
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetHealth(float newHealth)
    {
        currentHealth = newHealth;
        Debug.Log($"[HEALTH] {gameObject.name} health set to {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
}