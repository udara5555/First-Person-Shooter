using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Visual")]
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;
    public float hitDuration = 0.1f;

    private float hitTimer = 0f;

    void Start()
    {
        if (crosshairImage == null)
        {
            crosshairImage = GetComponent<Image>();
        }

        if (crosshairImage != null)
        {
            crosshairImage.color = normalColor;
        }
    }

    void Update()
    {
        // Countdown hit timer
        if (hitTimer > 0)
        {
            hitTimer -= Time.deltaTime;

            if (hitTimer <= 0)
            {
                ResetCrosshair();
            }
        }
    }

    public void OnHit()
    {
        hitTimer = hitDuration;
        if (crosshairImage != null)
        {
            crosshairImage.color = hitColor;
        }
    }

    void ResetCrosshair()
    {
        if (crosshairImage != null)
        {
            crosshairImage.color = normalColor;
        }
    }
}