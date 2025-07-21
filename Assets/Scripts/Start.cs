using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
public class Start : NetworkBehaviour
{
    public void SetStartPoint(Vector2 startHere, ulong clientId)
    {
        Debug.Log("Setting start point for this client: " + clientId);
        GameManager.Instance.SubmitStartPositionServerRpc(clientId,startHere);
    }

}
