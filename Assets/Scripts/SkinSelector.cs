using UnityEngine;
using UnityEngine.UI;

public class SkinSelector : MonoBehaviour
{
    [Header("Skin UI Elements")]
    public Button prevSkinBtn;
    public Button nextSkinBtn;
    public Image skinPreviewImage;

    [Header("Skin Preview Sprites")]
    public Sprite skin1Sprite;
    public Sprite skin2Sprite;

    [Header("Skin Materials")]
    public Material skin1Material;
    public Material skin2Material;

    private SkinData.SkinType currentSkin;

    private void Start()
    {
        prevSkinBtn.onClick.AddListener(PreviousSkin);
        nextSkinBtn.onClick.AddListener(NextSkin);

        currentSkin = SkinData.SelectedSkin;
        UpdateSkinDisplay();
    }

    private void NextSkin()
    {
        int skinIndex = SkinData.GetSkinIndex(currentSkin);
        skinIndex++;

        if (skinIndex > 1)
        {
            skinIndex = 0;
        }

        currentSkin = SkinData.GetSkinFromIndex(skinIndex);
        SkinData.SelectedSkin = currentSkin;
        UpdateSkinDisplay();
        Debug.Log("Switched to: " + SkinData.GetSkinName(currentSkin));
    }

    private void PreviousSkin()
    {
        int skinIndex = SkinData.GetSkinIndex(currentSkin);
        skinIndex--;

        if (skinIndex < 0)
        {
            skinIndex = 1;
        }

        currentSkin = SkinData.GetSkinFromIndex(skinIndex);
        SkinData.SelectedSkin = currentSkin;
        UpdateSkinDisplay();
        Debug.Log("Switched to: " + SkinData.GetSkinName(currentSkin));
    }

    private void UpdateSkinDisplay()
    {
        if (skinPreviewImage != null)
        {
            skinPreviewImage.sprite = currentSkin == SkinData.SkinType.Skin1 ? skin1Sprite : skin2Sprite;
        }
    }
}