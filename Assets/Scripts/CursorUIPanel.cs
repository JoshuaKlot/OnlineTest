using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class CursorUIManager : NetworkBehaviour
{
    [SerializeField] private Button readyButton;

    private void Start()
    {
        readyButton.onClick.AddListener(() =>
        {
                Debug.Log("Ready button pressed. Marking player as done.");
                GameManager.Instance.MarkPlayerDonePlacingCoinsServerRpc();

                
    
        });
    }
}
