using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : NetworkBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button startButton;

    private void Start()
    {
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            NetworkLogger.Log("[Host] Starting Host");

            SetStartButtonVisibility(); // Check visibility after host starts
        });

        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            NetworkLogger.Log("[Host] Starting Client");
            SetStartButtonVisibility(); // Hide on client
        });

        startButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsHost)
            {
                GameManager.Instance.StartGame(); // Call host-side start logic
                //startButton.gameObject.SetActive(false); // Hide the button after pressing
            }
        });

        SetStartButtonVisibility(); // Set initial visibility before connection
    }

    private void SetStartButtonVisibility()
    {
        // Only show the start button if this instance is the host
        startButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);
    }
}
