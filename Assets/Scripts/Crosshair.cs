using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Visual")]
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;
    public float hitDuration = 0.1f;

    [Header("ADS")]
    public float normalScale = 1f;
    public float adsScale = 0.5f; // Smaller when aiming
    public float scaleSpeed = 5f;

    private float hitTimer = 0f;
    private Vector3 targetScale = Vector3.one;

    void Start()
    {
        if (crosshairImage == null)
        {
            crosshairImage = GetComponent<Image>();
        }

        if (crosshairImage != null)
        {
            crosshairImage.color = normalColor;
            targetScale = Vector3.one * normalScale;
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

        // Smoothly scale crosshair
        if (crosshairImage != null)
        {
            crosshairImage.transform.localScale = Vector3.Lerp(
                crosshairImage.transform.localScale,
                targetScale,
                Time.deltaTime * scaleSpeed
            );
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

    public void SetAiming(bool isAiming)
    {
        targetScale = Vector3.one * (isAiming ? adsScale : normalScale);
    }

    void ResetCrosshair()
    {
        if (crosshairImage != null)
        {
            crosshairImage.color = normalColor;
        }
    }
}