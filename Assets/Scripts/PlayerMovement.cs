
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float vSpeed;
    [SerializeField] private float hSpeed;
    [SerializeField] private LayerMask obstacleMask;
    public bool isMoving;
    Rigidbody2D rb2d;
    private void Awake()
    {
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(this.transform.position);
        Vector3 snappedPosition = GridManager.Instance.GridToWorldCenter(gridPos);
        this.transform.position = snappedPosition;
    }
    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        if (!IsOwner) return; // Only allow local player to move their character

        float hMovemnent = Input.GetAxisRaw("Horizontal");
        float vMovement = Input.GetAxisRaw("Vertical");
        if(hMovemnent!=0)
        {
            Vector2 HorizontalMovement = new Vector2(hMovemnent, 0);
            MoveAlongGrid(HorizontalMovement);
        }
        else if(vMovement!=0)
        {
            Vector2 VerticalMovement = new Vector2(0,vMovement);
            MoveAlongGrid(VerticalMovement);
        }
    }

    void MoveAlongGrid(Vector2 direction)
    {
        Vector2Int targetGridPos = GridManager.Instance.FindAdjacentGrid(this.transform.position, direction);
        Vector3 worldTarget = GridManager.Instance.GridToWorldCenter(targetGridPos);
        if (!isMoving&&CanMove(direction))
        {
            StartCoroutine(LerpToPosition(worldTarget));
        }
    }
    bool CanMove(Vector2 direction)
    {
        Debug.DrawRay(this.transform.position, direction.normalized * 2f, Color.red, 2f);
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, direction.normalized, 2f, obstacleMask);
        return hit.collider == null; // If we hit something in obstacleMask, we can't move
    }

    private IEnumerator LerpToPosition(Vector3 targetPos)
    {
        isMoving = true;
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
        isMoving = false;
    }

}

