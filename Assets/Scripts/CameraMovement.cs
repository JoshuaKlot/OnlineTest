using UnityEngine;
using Unity.Netcode;
using System.Security.Cryptography;
using Unity.Cinemachine;
using System.Collections;
using System;

public class CameraMovement : NetworkBehaviour
{
    [SerializeField] private float vSpeed;
    [SerializeField] private float hSpeed;
    [SerializeField] private GameObject Camera;
    [SerializeField] private Vector2 SpawnPOint;
    [SerializeField, Range(0f, 1f)] private float followSpeed;
    Rigidbody2D rb2d;
    bool playerPhase = false;
    GameObject Player;

    private void Awake()
    {
        Debug.Log("The camera tracker has spawned");
        Camera = GameObject.Find("CmCamera");
        this.transform.position = SpawnPOint;
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
       // Debug.Log("vMovement is " + vMovement + " and hMovement is " + hMovemnent);
        float phMovement = Input.GetAxisRaw("Horizontal");
        float pvMovement = Input.GetAxisRaw("Vertical");
        rb2d.linearVelocity = new Vector2(hMovemnent * hSpeed, vSpeed * vMovement);
        if (Player.GetComponent<PlayerMovement>().isMoving)
        {
            Debug.Log("vMovement is " + pvMovement + " and hMovement is " + phMovement);
            if (phMovement != 0 || pvMovement != 0) {
                LerpToPlayer();

            }
        }
    }

    private void LerpToPlayer()
    {
        Vector3 pos = transform.position;
        Vector3 target = Player.transform.position;

        StartCoroutine(LerpToPosition(target));
        //transform.position = new Vector3(
        //    Mathf.Lerp(pos.x, target.x, followSpeed*Time.deltaTime),
        //    Mathf.Lerp(pos.y, target.y, followSpeed * Time.deltaTime),
        //    pos.z
        //);
    }

    private IEnumerator LerpToPosition(Vector3 targetPos)
    { 
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;
        float duration = 1f / hSpeed;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }

    public void FollowPlayer(GameObject player)
    {
        Player = player;
        playerPhase = true;
        LerpToPlayer();
    }

}
