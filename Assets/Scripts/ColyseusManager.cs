using Colyseus;
using Colyseus.Schema;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private Health localPlayerHealth;

    class RemoteData
    {
        public GameObject go;
        public Vector3 targetPos;
        public Quaternion targetRot;
        public Animator animator;
        public bool lastIsWalking = false;
        public string sessionId;
        public float lastHealth;
        public string currentSkin;
    }
    readonly Dictionary<string, RemoteData> remotes = new();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Start()
    {
        if (passedRoom != null)
        {
            room = passedRoom;
            client = passedClient;
            passedRoom = null;
            passedClient = null;
            Debug.Log("Using existing room connection: " + room.RoomId);
            Debug.Log("Server URL: " + ServerConfig.GetServerUrl());
        }
        else
        {
            Debug.LogError("No room passed to ColyseusManager!");
            return;
        }

        Debug.Log("Connected as: " + LobbyData.PlayerName + " | Room: " + room.RoomId);
        fpsController = localPlayer.GetComponent<FPSController>();
        localPlayerHealth = localPlayer.GetComponent<Health>();
        HookCallbacks();
    }

    public static void SetRoom(Room<MyRoomState> newRoom, Client newClient)
    {
        passedRoom = newRoom;
        passedClient = newClient;
    }

    public Room<MyRoomState> GetRoom()
    {
        return room;
    }

    public void OnLocalPlayerDied()
    {
        Debug.Log("Local player died! Returning to lobby...");
        ReturnToLobby();
    }

    void HookCallbacks()
    {
        var cb = Callbacks.Get(room);

        // NEW: Listen for timeRemaining changes from server
        /*cb.OnChange(room.State, () =>
        {
            Debug.Log($"Game state changed - TimeRemaining: {room.State.timeRemaining}, IsGameActive: {room.State.isGameActive}");
        });*/

        cb.OnAdd(state => state.players, (sessionId, player) =>
        {
            if (sessionId == room.SessionId) return;

            Debug.Log($"Spawning remote player {sessionId} at position ({player.x}, {player.y}, {player.z})");

            var go = Instantiate(remotePlayerPrefab);
            go.transform.position = new Vector3(player.x, player.y, player.z);
            go.tag = "RemotePlayer";
            go.name = $"RemotePlayer_{sessionId}";

            Debug.Log($"Remote player {sessionId} instantiated at {go.transform.position}");

            var animator = go.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"Remote player {sessionId} instantiated without an Animator component!");
            }

            var remoteComponent = go.AddComponent<RemotePlayerComponent>();
            remoteComponent.SessionId = sessionId;

            remotes[sessionId] = new RemoteData
            {
                go = go,
                targetPos = go.transform.position,
                targetRot = Quaternion.Euler(0, player.rotY, 0),
                animator = animator,
                lastIsWalking = player.isWalking,
                sessionId = sessionId,
                lastHealth = player.health,
                currentSkin = player.skin
            };

            if (animator != null)
            {
                animator.SetBool("isWalking", player.isWalking);
                Debug.Log($"Remote player {sessionId} spawned with isWalking: {player.isWalking}");
            }

            // Apply skin to remote player
            ApplySkinToRemotePlayer(go, player.skin);
            Debug.Log($"Remote player {sessionId} spawned with skin: {player.skin}");

            

            cb.OnChange(player, () =>
            {
                if (!remotes.TryGetValue(sessionId, out var rd)) return;

                rd.targetPos = new Vector3(player.x, player.y, player.z);
                rd.targetRot = Quaternion.Euler(0, player.rotY, 0);

                if (rd.animator != null && rd.lastIsWalking != player.isWalking)
                {
                    rd.animator.SetBool("isWalking", player.isWalking);
                    rd.lastIsWalking = player.isWalking;
                    Debug.Log($"Remote player {sessionId} isWalking changed to: {player.isWalking}");
                }

                // UPDATED: Handle skin changes with direct material assignment
                if (rd.currentSkin != player.skin)
                {
                    ApplySkinToRemotePlayer(rd.go, player.skin);
                    rd.currentSkin = player.skin;
                    Debug.Log($"Remote player {sessionId} skin changed to: {player.skin}");
                }

                if (rd.lastHealth != player.health)
                {
                    Debug.Log($"Remote player {sessionId} health changed from {rd.lastHealth} to {player.health}");
                    rd.lastHealth = player.health;

                    var remoteComponent = rd.go.GetComponent<RemotePlayerComponent>();
                    if (remoteComponent != null)
                        remoteComponent.UpdateHealth(player.health, player.maxHealth);
                }
            });
        });

        cb.OnRemove(state => state.players, (sessionId, player) =>
        {
            if (remotes.TryGetValue(sessionId, out var rd) && rd.go)
                Destroy(rd.go);
            remotes.Remove(sessionId);
        });

        room.OnMessage<PlayerDeathMessage>("playerDied", (message) =>
        {
            Debug.Log($"Player died: {message.playerId}");

            if (message.playerId == room.SessionId)
            {
                Debug.Log("Local player died! Returning to lobby...");
                ReturnToLobby();
            }
            else if (remotes.TryGetValue(message.playerId, out var rd))
            {
                if (rd.go != null)
                    Destroy(rd.go);
                remotes.Remove(message.playerId);
            }
        });

        room.OnMessage<PlayerDamagedMessage>("playerDamaged", (message) =>
        {
            Debug.Log($"Player damaged: ID={message.playerId}, Health={message.health}, Damage={message.damage}");
        });
    }

    // NEW METHOD: Apply skin directly by finding renderers
    private void ApplySkinToRemotePlayer(GameObject playerGo, string skinName)
    {
        Material skinMaterial = GetMaterialBySkinName(skinName);

        // Find all renderers in the player and its children
        Renderer[] renderers = playerGo.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            // Skip the health bar canvas
            if (renderer.gameObject.name.Contains("HealthBar")) continue;

            // Apply skin material to all materials in the renderer
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = skinMaterial;
            }
            renderer.materials = materials;
        }

        Debug.Log($"Applied skin '{skinName}' to remote player {playerGo.name}");
    }

    // NEW METHOD: Get material by skin name
    private Material GetMaterialBySkinName(string skinName)
    {
        // Reference materials from the local player
        var localSkinApplier = localPlayer?.GetComponent<PlayerSkinApplier>();

        if (localSkinApplier != null)
        {
            return skinName switch
            {
                "Skin1" => localSkinApplier.GetSkin1Material(),
                "Skin2" => localSkinApplier.GetSkin2Material(),
                _ => localSkinApplier.GetSkin1Material()
            };
        }

        // Fallback: try to find materials from the prefab
        var prefabSkinApplier = remotePlayerPrefab.GetComponent<PlayerSkinApplier>();
        if (prefabSkinApplier != null)
        {
            return skinName switch
            {
                "Skin1" => prefabSkinApplier.GetSkin1Material(),
                "Skin2" => prefabSkinApplier.GetSkin2Material(),
                _ => prefabSkinApplier.GetSkin1Material()
            };
        }

        Debug.LogError("Could not find skin materials!");
        return null;
    }

    void ReturnToLobby()
    {
        Time.timeScale = 1f; // Ensure time is running

        // Leave the room
        if (room != null)
        {
            room.Leave();
            room = null;
        }

        // Clear remotes
        remotes.Clear();

        // Load lobby scene
        Debug.Log("Loading Lobby scene...");
        SceneManager.LoadScene("Lobby");
    }

    void Update()
    {
        if (room == null || localPlayer == null) return;

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

        foreach (var kv in remotes)
        {
            var r = kv.Value;
            if (r.go != null)
            {
                r.go.transform.position = Vector3.Lerp(r.go.transform.position, r.targetPos, Time.deltaTime * positionLerp);
                r.go.transform.rotation = Quaternion.Lerp(r.go.transform.rotation, r.targetRot, Time.deltaTime * positionLerp);
            }
        }
    }
}

public class PlayerDamagedMessage
{
    public string playerId;
    public float health;
    public float damage;
}

public class PlayerDeathMessage
{
    public string playerId;
}