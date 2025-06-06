using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPhaseManager : NetworkBehaviour
{
    public Button resetButton;

    void Start()
    {
        // Hide the button if this client is not the host
        if (!NetworkManager.Singleton.IsHost)
        {
            resetButton.gameObject.SetActive(false);
            return; // Don't add the listener
        }

        // Only host reaches this point
        resetButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ResetGame();
        });
    }

    void Update()
    {
        // Not used, safe to delete or keep if needed later
    }
}
