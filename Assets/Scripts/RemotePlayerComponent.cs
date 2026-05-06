using UnityEngine;
using UnityEngine.UI;

public class RemotePlayerComponent : MonoBehaviour
{
    public string SessionId { get; set; }
    
    private Canvas healthBarCanvas;
    private Image healthBarFill;
    private float maxHealth = 100f;

    void Start()
    {
        CreateHealthBar();
    }

    void CreateHealthBar()
    {
        // Create canvas for health bar
        var canvasGO = new GameObject("HealthBarCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = new Vector3(0, 2.5f, 0);

        healthBarCanvas = canvasGO.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;

        var canvasRectTransform = canvasGO.GetComponent<RectTransform>();
        canvasRectTransform.sizeDelta = new Vector2(1, 0.2f);

        // Create background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform);
        bgGO.transform.localPosition = Vector3.zero;
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = Color.black;
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(1, 0.2f);

        // Create health fill
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(bgGO.transform);
        fillGO.transform.localPosition = Vector3.zero;
        healthBarFill = fillGO.AddComponent<Image>();
        healthBarFill.color = Color.green;
        var fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(1, 0.2f);
        fillRect.anchorMin = new Vector2(0, 0.5f);
        fillRect.anchorMax = new Vector2(0, 0.5f);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        this.maxHealth = maxHealth;
        if (healthBarFill != null)
        {
            float healthPercent = currentHealth / maxHealth;
            healthBarFill.fillAmount = healthPercent;
            
            // Change color based on health
            if (healthPercent > 0.5f)
                healthBarFill.color = Color.green;
            else if (healthPercent > 0.25f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }
    }
}