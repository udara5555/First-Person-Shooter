using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    [Header("Impact Mark")]
    public float markDuration = 10f;
    public float markScale = 0.5f;

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Destroy impact mark after duration
        if (Time.time - spawnTime > markDuration)
        {
            Destroy(gameObject);
        }
    }
}