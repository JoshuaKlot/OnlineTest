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
            // Check for both Start and Exit objects
            bool hasStart = false;
            bool hasExit = false;
            foreach (var obj in FindObjectsOfType<UniqueObject>())
            {
                if (obj.uniqueType == UniqueObject.UniqueType.Start)
                    hasStart = true;
                if (obj.uniqueType == UniqueObject.UniqueType.Exit)
                    hasExit = true;
            }

            if (!hasStart || !hasExit)
            {
                Debug.LogWarning("You must place both a start and an exit before readying up!");
                NetworkLogger.Instance.AddLog("You must place both a start and an exit before readying up!");
                return;
            }

            Debug.Log("Entances Set Up.");
            SendMsg.Instance.SetUpEntrances(NetworkManager.Singleton.LocalClientId);
            GameManager.Instance.MarkPlayerDonePlacingCoinsServerRpc();
        });
    }
}
