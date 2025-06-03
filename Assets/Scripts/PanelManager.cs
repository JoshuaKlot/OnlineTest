using UnityEngine;
using Unity.Netcode;

public class PanelManager : NetworkBehaviour
{
    public static PanelManager Instance;

    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject cursorPhasePanel;
    [SerializeField] private GameObject playerPhasePanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowLobbyOnClients()
    {
        ShowLobbyClientRpc();
    }

    public void ShowCursorPhaseOnClients()
    {
        ShowCursorPhaseClientRpc();
    }

    public void ShowPlayerPhaseOnClients()
    {
        ShowPlayerPhaseClientRpc();
    }

    [ClientRpc]
    private void ShowLobbyClientRpc()
    {
        HideAll();
        lobbyPanel?.SetActive(true);
    }

    [ClientRpc]
    private void ShowCursorPhaseClientRpc()
    {
        HideAll();
        cursorPhasePanel?.SetActive(true);
    }

    [ClientRpc]
    private void ShowPlayerPhaseClientRpc()
    {
        HideAll();
        playerPhasePanel?.SetActive(true);
    }

    private void HideAll()
    {
        lobbyPanel?.SetActive(false);
        cursorPhasePanel?.SetActive(false);
        playerPhasePanel?.SetActive(false);
    }
}
