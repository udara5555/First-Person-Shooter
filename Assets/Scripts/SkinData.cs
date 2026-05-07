using UnityEngine;

public static class SkinData
{
    public enum SkinType
    {
        Skin1 = 0,
        Skin2 = 1
    }

    public static SkinType SelectedSkin = SkinType.Skin1;

    public static string GetSkinName(SkinType skin)
    {
        return skin switch
        {
            SkinType.Skin1 => "Skin1",
            SkinType.Skin2 => "Skin2",
            _ => "Skin1"
        };
    }

    public static int GetSkinIndex(SkinType skin)
    {
        return (int)skin;
    }

    public static SkinType GetSkinFromIndex(int index)
    {
        return index switch
        {
            0 => SkinType.Skin1,
            1 => SkinType.Skin2,
            _ => SkinType.Skin1
        };
    }
}