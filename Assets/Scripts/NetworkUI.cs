using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button startButton;

    private void Start()
    {
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });

        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });

        startButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsHost)
            {
                GameManager.Instance.StartGame(); // Call host-side start logic
                startButton.gameObject.SetActive(false); // Hide the button after pressing
            }
        });
    }
}
