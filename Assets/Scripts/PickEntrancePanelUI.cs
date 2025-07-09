using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class PickEntrancePanelUI : NetworkBehaviour
{

    [SerializeField] private Button readyButton;
    private void Start()
    {

            readyButton.onClick.AddListener(() =>
            {
                Debug.Log("Ready button pressed. Marking player as done.");
                SendMsg.Instance.SetUpEntrances(NetworkManager.Singleton.LocalClientId);
                GameManager.Instance.SetUpObsticalsServerRpc();
            });
   
    }
}
