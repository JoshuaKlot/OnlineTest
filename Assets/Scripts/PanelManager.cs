using UnityEngine;

public class PanelManager : MonoBehaviour
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

    public void ShowLobby()
    {
        HideAll();
        lobbyPanel.SetActive(true);
    }

    public void ShowCursorPhase()
    {
        HideAll();
        cursorPhasePanel.SetActive(true);
    }

    public void ShowPlayerPhase()
    {
        HideAll();
        playerPhasePanel.SetActive(true);
    }

    private void HideAll()
    {
        lobbyPanel?.SetActive(false);
        cursorPhasePanel?.SetActive(false);
        playerPhasePanel?.SetActive(false);
    }
}
