using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
public class Start : NetworkBehaviour
{
    void Awake()
    {
        GameManager.Instance.SetStartPoint(this.gameObject.GetComponent<NetworkObject>().OwnerClientId,this.transform.position);
    }
}
