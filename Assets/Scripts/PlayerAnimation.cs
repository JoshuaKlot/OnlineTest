using UnityEditor.Animations;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator ani;
    [SerializeField] private PlayerMovement move;
    private Animator shirtAni;
    [SerializeField] private GameObject shirt;
    private int direction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shirtAni=shirt.GetComponent<Animator>();
        direction = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ani.SetInteger("direction",direction);
        shirtAni.SetInteger("direction",direction);
        bool walking = (move.isMoving);
        ani.SetBool("walk",walking);
        shirtAni.SetBool("walk", walking);


        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        if (!walking)
        {
            Debug.Log("Standing still");
            // Determine direction based on input
            if (vertical < 0)
                direction = 0; // Down
            else if (vertical > 0)
                direction = 1; // Up
            else if (horizontal != 0)
            {
                direction = 2; // Side
                sprite.flipX = horizontal > 0;
            }
        }
        else
        {
            Debug.Log("walking");
        }
    }
}
