
using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
public class CinemachineSetUp : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera cmCamera;

    public void SetUpTracking(GameObject tracker)
    {
        cmCamera.LookAt = tracker.transform;
    }
}
