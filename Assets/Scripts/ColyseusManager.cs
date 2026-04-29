using Colyseus;
using Colyseus.Schema;
using System.Collections.Generic;
using UnityEngine;

public class ColyseusManager : MonoBehaviour
{
    public static ColyseusManager Instance;

    [Header("Server")]
    public string serverUrl = "ws://127.0.0.1:2567";
    public string roomName = "my_room";

    [Header("Scene Refs")]
    public Transform localPlayer;
    public GameObject remotePlayerPrefab;

    public float sendInterval = 0.05f;
    public float positionLerp = 12f;

    private Client client;
    private Room<MyRoomState> room;
    private float sendTimer;

    class RemoteData
    {
        public GameObject go;
        public Vector3 targetPos;
        public Quaternion targetRot;
    }
    readonly Dictionary<string, RemoteData> remotes = new();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    async void Start()
    {
        client = new Client(serverUrl);

        if (LobbyData.IsCreator)
            room = await client.JoinById<MyRoomState>(LobbyData.RoomId);
        else
            room = await client.JoinById<MyRoomState>(LobbyData.RoomId);

        Debug.Log("Joined as: " + LobbyData.PlayerName + " | Room: " + room.RoomId);
        HookCallbacks();
    }

    void HookCallbacks()
    {
        var cb = Callbacks.Get(room);

        cb.OnAdd(state => state.players, (sessionId, player) =>
        {
            if (sessionId == room.SessionId) return;

            var go = Instantiate(remotePlayerPrefab);
            go.transform.position = new Vector3(player.x, player.y, player.z);

            remotes[sessionId] = new RemoteData
            {
                go = go,
                targetPos = go.transform.position,
                targetRot = Quaternion.Euler(0, player.rotY, 0)
            };

            cb.OnChange(player, () =>
            {
                if (!remotes.TryGetValue(sessionId, out var rd)) return;
                rd.targetPos = new Vector3(player.x, player.y, player.z);
                rd.targetRot = Quaternion.Euler(0, player.rotY, 0);
            });
        });

        cb.OnRemove(state => state.players, (sessionId, player) =>
        {
            if (remotes.TryGetValue(sessionId, out var rd) && rd.go)
                Destroy(rd.go);
            remotes.Remove(sessionId);
        });
    }

    void Update()
    {


        if (room == null || localPlayer == null) return;

        // Send position
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            Vector3 pos = localPlayer.position;
            float rotY = localPlayer.eulerAngles.y;

            room.Send("move", new Dictionary<string, object> {
                { "x", pos.x }, { "y", pos.y }, { "z", pos.z },
                { "rotY", rotY }
            });
        }

        // Interpolate remotes
        foreach (var kv in remotes)
        {
            var r = kv.Value;
            r.go.transform.position = Vector3.Lerp(r.go.transform.position, r.targetPos, Time.deltaTime * positionLerp);
            r.go.transform.rotation = Quaternion.Lerp(r.go.transform.rotation, r.targetRot, Time.deltaTime * positionLerp);
        }
    }
}