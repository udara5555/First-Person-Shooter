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
    private PlayerUI playerUI;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();

        // Find PlayerUI on the same GameObject
        playerUI = GetComponent<PlayerUI>();

        // If not found, try to find it in the scene
        if (playerUI == null)
        {
            playerUI = Object.FindFirstObjectByType<PlayerUI>();
        }

        if (playerUI == null)
        {
            Debug.LogWarning("PlayerUI not found! Damage panel won't show.");
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Show damage panel if this is the local player
        if (playerUI != null)
        {
            playerUI.ShowDamagePanel();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
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
        // Show damage panel if health decreased (hit by another player)
        if (newHealth < currentHealth && playerUI != null)
        {
            playerUI.ShowDamagePanel();
        }

        currentHealth = newHealth;
        Debug.Log($"[HEALTH] {gameObject.name} health set to {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
}