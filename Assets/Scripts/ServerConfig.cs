using UnityEngine;

public class ServerConfig : MonoBehaviour
{
    public static ServerConfig Instance { get; private set; }

    [SerializeField]
    public string serverUrl = "ws://127.0.0.1:2567";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static string GetServerUrl()
    {
        return Instance != null ? Instance.serverUrl : "ws://127.0.0.1:2567";
    }
}