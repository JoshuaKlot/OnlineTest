
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float vSpeed;
    [SerializeField] private float hSpeed;
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
        Vector2 nextPosition = GridManager.Instance.FindAdjacentGrid(this.transform.position, direction);
        StartCoroutine(LerpToPosition(nextPosition));
    }

    private IEnumerator LerpToPosition(Vector2 nextPosition)
    {
        yield return new WaitUntil(() => new Vector2(this.transform.position.x, this.transform.position.y) == nextPosition);
        this.transform.position = Vector2.Lerp(new Vector2(this.transform.position.x, this.transform.position.y), nextPosition,hSpeed*Time.deltaTime);
    }
}

