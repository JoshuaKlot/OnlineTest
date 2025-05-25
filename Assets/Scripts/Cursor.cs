using UnityEngine;
using Unity.Netcode;

public class Cursor : NetworkBehaviour
{
    [SerializeField] private GameObject coins;
    [SerializeField] private int numOfCoins;
    [SerializeField] private GameObject mainLevel;

    void Update()
    {
        if (!IsOwner) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(cursorWorldPosition.x, cursorWorldPosition.y, 0);

        if (Input.GetMouseButtonDown(0) && numOfCoins > 0)
        {
            SpawnCoinServerRpc(transform.position);
            numOfCoins--;
        }

        if (numOfCoins == 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPlayerPhaseServerRpc();
            }
        }
    }

    [ServerRpc]
    private void SpawnCoinServerRpc(Vector3 spawnPosition)
    {
        GameObject placedCoins = Instantiate(coins, spawnPosition, Quaternion.identity);
        placedCoins.transform.parent = mainLevel.transform;

        NetworkObject networkObject = placedCoins.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true); // true if you want to spawn with ownership
        }
    }
}
