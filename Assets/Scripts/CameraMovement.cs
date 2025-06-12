using UnityEngine;
using Unity.Netcode;
using System.Security.Cryptography;
using Unity.Cinemachine;

public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private float vSpeed;
    [SerializeField] private float hSpeed;
    [SerializeField] private GameObject Camera;
    Rigidbody2D rb2d;
    bool playerPhase = false;
    GameObject Player;

    private void Awake()
    {
        Debug.Log("The camera tracker has spawned");
        Camera = GameObject.Find("CmCamera");
        
    }
    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (Camera != null)
        {
            Camera.GetComponent<CinemachineSetUp>().SetUpTracking(this.transform.gameObject);
        }
        else
            Debug.Log("Camera not found"); 
    }

    void Update()
    {

        if (!IsOwner) return; // Only allow local player to move their character

        float hMovemnent = Input.GetAxisRaw("Camera Horizontal");
        float vMovement = Input.GetAxisRaw("Camera Vertical");
        Debug.Log("vMovement is " + vMovement + " and hMovement is " + hMovemnent);
        float phMovement = Input.GetAxisRaw("Horizontal");
        float pvMovement = Input.GetAxisRaw("Vertical");
        rb2d.linearVelocity = new Vector2(hMovemnent * hSpeed, vSpeed * vMovement);
        if (playerPhase)
        {
            if (phMovement != 0 && pvMovement != 0) {
                transform.position = Vector3.Lerp(this.transform.position, Player.transform.position,2);
            }
        }
    }

    public void FollowPlayer(GameObject player)
    {
        Player = player;
        playerPhase = true;
        transform.position=Player.transform.position;
    }

}
