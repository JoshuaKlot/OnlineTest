//using UnityEditor.Animations;
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
    private bool walking;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shirtAni=shirt.GetComponent<Animator>();
        direction = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        if (vertical < 0)
            direction = 0; // Down
        else if (vertical > 0)
            direction = 1; // Up
        else if (horizontal != 0)
        {
            direction = 2; // Side
            sprite.flipX = horizontal > 0;
        } 
        //Debug.Log("direction: "+direction);
        walking = (move.isMoving);
        animateDirection();
        animateWalking();


    }

    void animateDirection()
    {
        ani.SetInteger("direction", direction);
        shirtAni.SetInteger("direction", direction);
    }

    void animateWalking()
    {
        ani.SetBool("walk", walking);
        shirtAni.SetBool("walk", walking);

    }
}
