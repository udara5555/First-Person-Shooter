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

    [Header("Scene")]
    public string gameSceneName = "Map";

    private Client client;
    private Room<MyRoomState> room;
    private string serverUrl = "ws://127.0.0.1:2567";

    void Start()
    {
        client = new Client(serverUrl);
        playerNameInput.text = "Player_" + Random.Range(1000, 9999);

        createRoomBtn.onClick.AddListener(CreateRoom);
        joinByIdBtn.onClick.AddListener(JoinRoom);
        startGameBtn.onClick.AddListener(StartGame);

        startGameBtn.gameObject.SetActive(false);
    }

    async void CreateRoom()
    {
        string playerName = playerNameInput.text.Trim();

        room = await client.Create<MyRoomState>("my_room", new Dictionary<string, object> {
            { "playerName", playerName }
        });

        LobbyData.RoomId = room.RoomId;
        LobbyData.PlayerName = playerName;
        LobbyData.IsCreator = true;

        roomIdText.text = "Room ID: " + room.RoomId;
        startGameBtn.gameObject.SetActive(true);

        Debug.Log("Room created: " + room.RoomId);
    }

    async void JoinRoom()
    {
        string code = roomIdInput.text.Trim();
        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(code)) { Debug.LogError("Enter a room ID"); return; }

        room = await client.JoinById<MyRoomState>(code, new Dictionary<string, object> {
            { "playerName", playerName }
        });

        LobbyData.RoomId = room.RoomId;
        LobbyData.PlayerName = playerName;
        LobbyData.IsCreator = false;

        roomIdText.text = "Room ID: " + room.RoomId;
        startGameBtn.gameObject.SetActive(true);

        Debug.Log("Joined room: " + room.RoomId);
    }

    void StartGame()
    {
        if (room == null) return;

        room.Send("joinGame");

        // Pass the room to ColyseusManager before loading scene
        ColyseusManager.SetRoom(room, client);

        SceneManager.LoadScene(gameSceneName);
    }
}