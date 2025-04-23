using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float vSpeed;
    [SerializeField] private float hSpeed;
    Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float hMovemnent = Input.GetAxisRaw("Horizontal");
        float vMovement = Input.GetAxisRaw("Vertical");
        rb2d.velocity = new Vector2(hMovemnent * hSpeed, vSpeed * vMovement);
    }
}
