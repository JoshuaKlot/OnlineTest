using UnityEngine;

public class Exit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {

        //if (!IsServer) return;
        
        NetworkObject playerObj = col.GetComponent<NetworkObject>();
        if (playerObj == null) return;

        ulong triggeringClientId = playerObj.OwnerClientId;

        // Allow only the intended player to collect the coin
        if (visibility.CheckVisibility(triggeringClientId))
        {
            GameManager.Instance.SendExitReachedMsg(triggeringClientId);
            NetworkObject.Despawn();
        }
    }
}
