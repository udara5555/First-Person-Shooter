using UnityEngine;

public class RemotePlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Death")]
    public GameObject deathEffect;
    public AudioClip deathSound;

    private AudioSource audioSource;
    private RemotePlayerComponent remotePlayerComponent;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        remotePlayerComponent = GetComponent<RemotePlayerComponent>();
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

        Debug.Log($"Remote player {gameObject.name} died!");

        // Play death sound
        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

        // Spawn death effect
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, transform.rotation);

        // Notify the server that this remote player was killed
        if (ColyseusManager.Instance != null)
        {
            var room = ColyseusManager.Instance.GetRoom();
            if (room != null && remotePlayerComponent != null)
            {
                room.Send("playerKilled", new { playerId = remotePlayerComponent.SessionId });
                Debug.Log($"Sent playerKilled message for {remotePlayerComponent.SessionId}");
            }
        }

        // Just destroy this remote player from the scene (don't return to lobby)
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
}