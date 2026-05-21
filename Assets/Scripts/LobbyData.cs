using System.Collections.Generic;

public static class LobbyData
{
    public static string RoomId = "";
    public static string PlayerName = "";
    public static bool IsCreator = false;
    public static List<string> SelectedWeapons = new List<string> { "ak47", "magnum" }; // Default weapons: AK-47 and Magnum
}