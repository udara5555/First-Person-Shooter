using UnityEngine;

public class PlayerSkinApplier : MonoBehaviour
{
    [Header("Material References")]
    public Material skin1Material;
    public Material skin2Material;

    [Header("Player Parts")]
    public Renderer armsRenderer;      // Cube.010
    public Renderer bodyRenderer;      // Cube.011

    private void Start()
    {
        ApplySelectedSkin();
    }

    private void ApplySelectedSkin()
    {
        Material selectedMaterial = SkinData.SelectedSkin switch
        {
            SkinData.SkinType.Skin1 => skin1Material,
            SkinData.SkinType.Skin2 => skin2Material,
            _ => skin1Material
        };

        ApplyMaterial(selectedMaterial);
        Debug.Log("Applied skin: " + SkinData.GetSkinName(SkinData.SelectedSkin));
    }

    public void ApplySkinByName(string skinName)
    {
        Material selectedMaterial = skinName switch
        {
            "Skin1" => skin1Material,
            "Skin2" => skin2Material,
            _ => skin1Material
        };

        ApplyMaterial(selectedMaterial);
        Debug.Log("Applied skin: " + skinName);
    }

    private void ApplyMaterial(Material material)
    {
        if (armsRenderer != null)
        {
            armsRenderer.material = material;
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.material = material;
        }
    }

    // PUBLIC GETTER METHODS for ColyseusManager
    public Material GetSkin1Material()
    {
        return skin1Material;
    }

    public Material GetSkin2Material()
    {
        return skin2Material;
    }
}