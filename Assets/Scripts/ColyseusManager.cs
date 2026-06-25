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
    private WeaponManager weaponManager;
    private string lastSentWeaponId = "";
    private bool lastSentIsSprinting = false;
    private bool lastSentIsReloading = false;
    private bool initialized = false;

    class RemoteData
    {
        public GameObject go;
        public Vector3 targetPos;
        public Quaternion targetRot;
        public Animator animator;
        public bool lastIsWalking = false;
        public bool lastIsSprinting = false;
        public bool lastIsReloading = false;
        public string sessionId;
        public float lastHealth;
        public string currentSkin;
        public string currentWeaponId;
        public RemotePlayerComponent remoteComponent;
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
        Debug.Log($"[START] MY SESSION ID: {room.SessionId}");

        fpsController = localPlayer.GetComponent<FPSController>();
        localPlayerHealth = localPlayer.GetComponent<Health>();
        weaponManager = localPlayer.GetComponent<WeaponManager>();

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

        room.OnMessage<WeaponSwitchedMessage>("weaponSwitched", (message) =>
        {
            Debug.Log($"\n>>> weaponSwitched MESSAGE RECEIVED <<<");
            Debug.Log($"SenderSessionId: {message.playerId}");
            Debug.Log($"MySessionId: {room.SessionId}");
            Debug.Log($"WeaponId: {message.weaponId}");
            Debug.Log($"Is this my weapon? {message.playerId == room.SessionId}");

            if (message.playerId != room.SessionId && remotes.TryGetValue(message.playerId, out var rd))
            {
                Debug.Log($"Applying weapon to remote player: {rd.go.name}");
                ApplyWeaponToRemotePlayer(rd.go, message.weaponId);
                rd.currentWeaponId = message.weaponId;
                Debug.Log($"✓ Applied\n");
            }
            else if (message.playerId == room.SessionId)
            {
                Debug.Log($"Ignoring my own weapon change\n");
            }
            else
            {
                Debug.LogWarning($"Remote player {message.playerId} NOT FOUND\n");
            }
        });

        cb.OnAdd(state => state.players, (sessionId, player) =>
        {
            Debug.Log($"\n>>> OnAdd CALLBACK <<<");
            Debug.Log($"PlayerSessionId: {sessionId}");
            Debug.Log($"MySessionId: {room.SessionId}");

            if (sessionId == room.SessionId)
            {
                Debug.Log($">>> This is MY player, SKIPPING\n");
                return;
            }

            Debug.Log($">>> Spawning remote player {sessionId}");
            Debug.Log($"Initial weapon from server: {player.currentWeaponId}");

            var go = Instantiate(remotePlayerPrefab);
            go.transform.position = new Vector3(player.x, player.y, player.z);
            go.tag = "RemotePlayer";
            go.name = $"RemotePlayer_{sessionId}";

            var animator = go.GetComponent<Animator>();
            var remoteComponent = go.AddComponent<RemotePlayerComponent>();
            remoteComponent.SessionId = sessionId;

            remotes[sessionId] = new RemoteData
            {
                go = go,
                targetPos = go.transform.position,
                targetRot = Quaternion.Euler(0, player.rotY, 0),
                animator = animator,
                lastIsWalking = player.isWalking,
                lastIsSprinting = player.isSprinting,
                lastIsReloading = player.isReloading,
                sessionId = sessionId,
                lastHealth = player.health,
                currentSkin = player.skin,
                currentWeaponId = player.currentWeaponId,
                remoteComponent = remoteComponent
            };

            if (animator != null)
            {
                animator.SetBool("isWalking", player.isWalking);
                animator.SetBool("isSprinting", player.isSprinting);
                animator.SetBool("isReloading", player.isReloading);
            }

            ApplySkinToRemotePlayer(go, player.skin);
            ApplyWeaponToRemotePlayer(go, player.currentWeaponId);
            Debug.Log($"✓ Remote player {sessionId} spawned with weapon: {player.currentWeaponId}\n");

            cb.OnChange(player, () =>
            {
                if (!remotes.TryGetValue(sessionId, out var rd)) return;

                rd.targetPos = new Vector3(player.x, player.y, player.z);
                rd.targetRot = Quaternion.Euler(0, player.rotY, 0);

                if (rd.animator != null && rd.lastIsWalking != player.isWalking)
                {
                    rd.animator.SetBool("isWalking", player.isWalking);
                    rd.lastIsWalking = player.isWalking;
                    Debug.Log($"[{sessionId}] Animation: isWalking = {player.isWalking}");
                }

                if (rd.animator != null && rd.lastIsSprinting != player.isSprinting)
                {
                    rd.animator.SetBool("isSprinting", player.isSprinting);
                    rd.lastIsSprinting = player.isSprinting;
                    Debug.Log($"[{sessionId}] Animation: isSprinting = {player.isSprinting}");
                }

                if (rd.animator != null && rd.lastIsReloading != player.isReloading)
                {
                    rd.animator.SetBool("isReloading", player.isReloading);
                    rd.lastIsReloading = player.isReloading;
                    Debug.Log($"[{sessionId}] Animation: isReloading = {player.isReloading}");
                }

                if (rd.currentSkin != player.skin)
                {
                    ApplySkinToRemotePlayer(rd.go, player.skin);
                    rd.currentSkin = player.skin;
                }

                // WEAPON CHANGE DETECTION
                if (rd.currentWeaponId != player.currentWeaponId)
                {
                    Debug.Log($"\n>>> WEAPON CHANGE DETECTED <<<");
                    Debug.Log($"RemotePlayer: {sessionId}");
                    Debug.Log($"OldWeapon: {rd.currentWeaponId}");
                    Debug.Log($"NewWeapon: {player.currentWeaponId}");
                    Debug.Log($"GameObject: {rd.go.name}");
                    ApplyWeaponToRemotePlayer(rd.go, player.currentWeaponId);
                    rd.currentWeaponId = player.currentWeaponId;
                    Debug.Log($"✓ Weapon applied\n");
                }

                if (rd.lastHealth != player.health)
                {
                    rd.lastHealth = player.health;
                    if (rd.remoteComponent != null)
                        rd.remoteComponent.UpdateHealth(player.health, player.maxHealth);
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
            if (message.playerId == room.SessionId)
            {
                OnLocalPlayerDied();
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
            if (message.playerId == room.SessionId && localPlayerHealth != null)
            {
                localPlayerHealth.SetHealth(message.health);
            }
        });

        room.OnMessage<PlayerShootMessage>("playerShoot", (message) =>
        {
            if (remotes.TryGetValue(message.playerId, out var rd) && rd.remoteComponent != null)
            {
                rd.remoteComponent.PlayShootEffects();
            }
        });
    }

    private void ApplySkinToRemotePlayer(GameObject playerGo, string skinName)
    {
        Material skinMaterial = GetMaterialBySkinName(skinName);
        Renderer[] renderers = playerGo.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            if (renderer.gameObject.name.Contains("HealthBar")) continue;

            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = skinMaterial;
            }
            renderer.materials = materials;
        }
    }

    private void ApplyWeaponToRemotePlayer(GameObject playerGo, string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId))
            weaponId = "ak47";

        Debug.Log($"\n[ApplyWeapon] Applying '{weaponId}' to {playerGo.name}");

        // Find Gun container - try multiple paths
        Transform gunContainer = null;

        // Try direct child first (remote player structure)
        gunContainer = playerGo.transform.Find("Gun");
        if (gunContainer != null)
        {
            Debug.Log($"✓ Found Gun as direct child of {playerGo.name}");
        }

        // Try under Spine1 (remote player structure)
        if (gunContainer == null)
        {
            Transform spine1 = playerGo.transform.Find("Spine1");
            if (spine1 != null)
            {
                gunContainer = spine1.Find("Gun");
                if (gunContainer != null)
                {
                    Debug.Log($"✓ Found Gun under Spine1");
                }
            }
        }

        // Try under Main Camera (local player structure)
        if (gunContainer == null)
        {
            Transform spine1 = playerGo.transform.Find("Spine1");
            if (spine1 != null)
            {
                Transform mainCamera = spine1.Find("Main Camera");
                if (mainCamera != null)
                {
                    gunContainer = mainCamera.Find("Gun");
                    if (gunContainer != null)
                    {
                        Debug.Log($"✓ Found Gun under Main Camera");
                    }
                }
            }
        }

        // Recursive search as last resort
        if (gunContainer == null)
        {
            Transform[] allTransforms = playerGo.GetComponentsInChildren<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name == "Gun")
                {
                    gunContainer = t;
                    Debug.Log($"✓ Found Gun via recursive search");
                    break;
                }
            }
        }

        if (gunContainer == null)
        {
            Debug.LogError($"✗ Gun container NOT FOUND in {playerGo.name}!");
            return;
        }

        Dictionary<string, int> weaponIndexMap = new Dictionary<string, int>
        {
            { "mp5", 0 },
            { "shotgun", 1 },
            { "smg", 2 },
            { "uzi", 3 },
            { "m16", 4 },
            { "sniper", 5 },
            { "magnum", 6 },
            { "ak47", 7 },
            { "lmg", 8 }
        };

        // Deactivate all weapons
        for (int i = 0; i < gunContainer.childCount; i++)
        {
            gunContainer.GetChild(i).gameObject.SetActive(false);
        }

        // Get weapon index
        if (!weaponIndexMap.TryGetValue(weaponId, out int gunIndex))
        {
            Debug.LogError($"✗ Weapon '{weaponId}' not in map!");
            return;
        }

        // Validate index
        if (gunIndex >= gunContainer.childCount)
        {
            Debug.LogError($"✗ Index {gunIndex} out of range! Available: 0-{gunContainer.childCount - 1}");
            return;
        }

        // Activate weapon
        Transform weapon = gunContainer.GetChild(gunIndex);
        weapon.gameObject.SetActive(true);
        Debug.Log($"✓ Activated {weapon.name} at index {gunIndex}\n");
    }

    private Material GetMaterialBySkinName(string skinName)
    {
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

        return null;
    }

    void ReturnToLobby()
    {
        Time.timeScale = 1f;

        // Re-enable cursor for lobby UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (room != null)
        {
            room.Leave();
            room = null;
        }

        remotes.Clear();

        SceneManager.LoadScene("Lobby");
    }

    void Update()
    {
        // Initialize after first frame to ensure all Start() methods have run
        if (!initialized && weaponManager != null && room != null)
        {
            initialized = true;

            string initialWeapon = weaponManager.GetCurrentWeaponId();
            Debug.Log($"\n[INIT] === INITIAL WEAPON SYNC ===");
            Debug.Log($"[INIT] My session: {room.SessionId}");
            Debug.Log($"[INIT] Initial weapon: {initialWeapon}");

            room.Send("switchWeapon", new Dictionary<string, object> {
                { "weaponId", initialWeapon }
            });
            lastSentWeaponId = initialWeapon;
            Debug.Log($"[INIT] ✓ Sent\n");
        }

        if (room == null || localPlayer == null) return;

        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            Vector3 pos = localPlayer.position;
            float rotY = localPlayer.eulerAngles.y;
            bool isWalking = fpsController != null ? fpsController.GetIsMoving() : false;
            bool isSprinting = fpsController != null ? fpsController.GetIsSprinting() : false;
            bool isReloading = weaponManager != null ? weaponManager.GetIsReloading() : false;

            // Only send if sprint or reload state changed
            if (isSprinting != lastSentIsSprinting || isReloading != lastSentIsReloading)
            {
                lastSentIsSprinting = isSprinting;
                lastSentIsReloading = isReloading;
                Debug.Log($"[ANIM_SYNC] isSprinting: {isSprinting}, isReloading: {isReloading}");
            }

            room.Send("move", new Dictionary<string, object> {
                { "x", pos.x }, { "y", pos.y }, { "z", pos.z },
                { "rotY", rotY }, { "isWalking", isWalking },
                { "isSprinting", isSprinting }, { "isReloading", isReloading }
            });
        }

        if (weaponManager != null)
        {
            string currentWeaponId = weaponManager.GetCurrentWeaponId();
            if (currentWeaponId != lastSentWeaponId)
            {
                lastSentWeaponId = currentWeaponId;
                Debug.Log($"[UPDATE] Sending my weapon switch: {currentWeaponId}");
                room.Send("switchWeapon", new Dictionary<string, object> {
                    { "weaponId", currentWeaponId }
                });
            }
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

public class WeaponSwitchedMessage
{
    public string playerId;
    public string weaponId;
}

public class PlayerShootMessage
{
    public string playerId;
    public string weaponId;
    public float originX;
    public float originY;
    public float originZ;
    public float dirX;
    public float dirY;
    public float dirZ;
}