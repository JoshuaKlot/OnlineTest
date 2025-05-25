using UnityEngine;
using Unity.Netcode;

public class MainLevel : NetworkBehaviour
{
    public static MainLevel Instance; // Optional singleton for easy access

    [SerializeField] private GameObject Cursor;
    [SerializeField] private GameObject Player;

    public NetworkVariable<bool> playerPhase = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (IsServer) // Only server should instantiate synced objects
        {
            GameObject cursorObj = Instantiate(Cursor);
            cursorObj.GetComponent<NetworkObject>().Spawn();
        }
    }

    void Update()
    {
        if (IsServer && playerPhase.Value && transform.childCount == 0)
        {
            Debug.Log("All coins collected");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerPhaseServerRpc()
    {
        playerPhase.Value = true;
    }
}
