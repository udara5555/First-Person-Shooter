using UnityEngine.UI;
using TMPro;
using Colyseus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text roomIdText;
    public TMP_InputField roomIdInput;
    public TMP_InputField playerNameInput;
    public Button createRoomBtn;
    public Button joinByIdBtn;
    public Button startGameBtn;
    public Button copyCodeBtn;

    [Header("Scene")]
    public string gameSceneName = "Map";

    private Client client;
    private Room<MyRoomState> room;

    void Start()
    {
        client = new Client(ServerConfig.GetServerUrl());
        playerNameInput.text = "Player_" + Random.Range(1000, 9999);

        createRoomBtn.onClick.AddListener(CreateRoom);
        joinByIdBtn.onClick.AddListener(JoinRoom);
        startGameBtn.onClick.AddListener(StartGame);
        copyCodeBtn.onClick.AddListener(CopyRoomId);

        startGameBtn.gameObject.SetActive(false);
        copyCodeBtn.gameObject.SetActive(false);
    }

    async void CreateRoom()
    {
        string playerName = playerNameInput.text.Trim();
        string selectedSkin = SkinData.GetSkinName(SkinData.SelectedSkin);

        room = await client.Create<MyRoomState>("my_room", new Dictionary<string, object> {
            { "playerName", playerName },
            { "skin", selectedSkin }
        });

        LobbyData.RoomId = room.RoomId;
        LobbyData.PlayerName = playerName;
        LobbyData.IsCreator = true;

        roomIdText.text = "Room ID: " + room.RoomId;
        startGameBtn.gameObject.SetActive(true);
        copyCodeBtn.gameObject.SetActive(true);

        Debug.Log("Room created: " + room.RoomId + " | Skin: " + selectedSkin);
    }

    async void JoinRoom()
    {
        string code = roomIdInput.text.Trim();
        string playerName = playerNameInput.text.Trim();
        string selectedSkin = SkinData.GetSkinName(SkinData.SelectedSkin);

        if (string.IsNullOrEmpty(code)) { Debug.LogError("Enter a room ID"); return; }

        room = await client.JoinById<MyRoomState>(code, new Dictionary<string, object> {
            { "playerName", playerName },
            { "skin", selectedSkin }
        });

        LobbyData.RoomId = room.RoomId;
        LobbyData.PlayerName = playerName;
        LobbyData.IsCreator = false;

        roomIdText.text = "Room ID: " + room.RoomId;
        startGameBtn.gameObject.SetActive(true);
        copyCodeBtn.gameObject.SetActive(true);

        Debug.Log("Joined room: " + room.RoomId + " | Skin: " + selectedSkin);
    }

    void StartGame()
    {
        if (room == null) return;

        // Send startGame message to server (instead of joinGame)
        room.Send("startGame");

        // Pass the room to ColyseusManager before loading scene
        ColyseusManager.SetRoom(room, client);

        SceneManager.LoadScene(gameSceneName);
    }

    void CopyRoomId()
    {
        if (room == null) return;

        JSLibHolder.CopyToClipboard(room.RoomId);
        Debug.Log("Room ID copied to clipboard: " + room.RoomId);
    }
}