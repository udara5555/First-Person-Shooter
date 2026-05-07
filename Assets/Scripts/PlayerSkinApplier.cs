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

        if (armsRenderer != null)
        {
            armsRenderer.material = selectedMaterial;
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.material = selectedMaterial;
        }

        Debug.Log("Applied skin: " + SkinData.GetSkinName(SkinData.SelectedSkin));
    }
}