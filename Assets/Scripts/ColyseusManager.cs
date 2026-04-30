using Colyseus;
using Colyseus.Schema;
using System.Collections.Generic;
using UnityEngine;

public class ColyseusManager : MonoBehaviour
{
    public static ColyseusManager Instance;

    [Header("Scene Refs")]
    public Transform localPlayer;
    public GameObject remotePlayerPrefab;

    public float sendInterval = 0.05f;
    public float positionLerp = 12f;

    private Client client;
    private Room<MyRoomState> room;
    private float sendTimer;
    private static Room<MyRoomState> passedRoom;
    private static Client passedClient;
    private FPSController fpsController;

    class RemoteData
    {
        public GameObject go;
        public Vector3 targetPos;
        public Quaternion targetRot;
        public Animator animator;
    }
    readonly Dictionary<string, RemoteData> remotes = new();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Start()
    {
        // Use the passed room if available, otherwise create new connection
        if (passedRoom != null)
        {
            room = passedRoom;
            client = passedClient;
            passedRoom = null;
            passedClient = null;
            Debug.Log("Using existing room connection: " + room.RoomId);
        }
        else
        {
            Debug.LogError("No room passed to ColyseusManager!");
            return;
        }

        Debug.Log("Connected as: " + LobbyData.PlayerName + " | Room: " + room.RoomId);
        fpsController = localPlayer.GetComponent<FPSController>();
        HookCallbacks();
    }

    public static void SetRoom(Room<MyRoomState> newRoom, Client newClient)
    {
        passedRoom = newRoom;
        passedClient = newClient;
    }

    void HookCallbacks()
    {
        var cb = Callbacks.Get(room);

        cb.OnAdd(state => state.players, (sessionId, player) =>
        {
            if (sessionId == room.SessionId) return;

            var go = Instantiate(remotePlayerPrefab);
            go.transform.position = new Vector3(player.x, player.y, player.z);

            var animator = go.GetComponent<Animator>();

            remotes[sessionId] = new RemoteData
            {
                go = go,
                targetPos = go.transform.position,
                targetRot = Quaternion.Euler(0, player.rotY, 0),
                animator = animator
            };

            cb.OnChange(player, () =>
            {
                if (!remotes.TryGetValue(sessionId, out var rd)) return;
                rd.targetPos = new Vector3(player.x, player.y, player.z);
                rd.targetRot = Quaternion.Euler(0, player.rotY, 0);

                // Update animation state
                if (rd.animator != null)
                {
                    rd.animator.SetBool("isWalking", player.isWalking);
                }
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

        // Send position and animation state
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            Vector3 pos = localPlayer.position;
            float rotY = localPlayer.eulerAngles.y;
            bool isWalking = fpsController != null ? fpsController.GetIsMoving() : false;

            room.Send("move", new Dictionary<string, object> {
                { "x", pos.x }, { "y", pos.y }, { "z", pos.z },
                { "rotY", rotY }, { "isWalking", isWalking }
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