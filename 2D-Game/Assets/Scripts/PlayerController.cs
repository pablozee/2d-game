using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed, jumpSpeed;
    [SerializeField] private LayerMask ground;

    private PlayerControls playerControls;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;
    private SpriteRenderer renderer;
    private bool isWalkingLeft = false;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerControls.Land.Jump.performed += _ => Jump();
    }

    // Update is called once per frame
    void Update()
    {
        // Read the movement value

        float movementInput = playerControls.Land.Move.ReadValue<float>();

        if (movementInput < 0 && !isWalkingLeft)
        {
            FlipX();
            isWalkingLeft = true;
        }

        if (movementInput > 0 && isWalkingLeft)
        {
            FlipX();
            isWalkingLeft = false;
        }

        if (movementInput != 0)
        {
            anim.SetFloat("Speed", movementInput);
        }

        // Move the player
        Vector3 currentPosition = transform.position;
        currentPosition.x += movementInput * speed * Time.deltaTime;
        transform.position = currentPosition;
    }

    private void Jump()
    {
        if (isGrounded())
        {
            Debug.Log("Jumping");
            rb.AddForce(new Vector2(0, jumpSpeed), ForceMode2D.Impulse);
            anim.SetTrigger("Jump");
        }
    }

    private bool isGrounded()
    {
        Vector2 topLeftPoint = transform.position;
        topLeftPoint.x -= col.bounds.extents.x;
        topLeftPoint.y += col.bounds.extents.y;

        Vector2 bottomRightPoint = transform.position;
        bottomRightPoint.x += col.bounds.extents.x;
        bottomRightPoint.y -= col.bounds.extents.y;

        Vector2 feetPos = transform.position;
        feetPos.y -= col.bounds.extents.y;

        return Physics2D.OverlapCircle(feetPos, .2f, ground);
    }

    void FlipX()
    {
        Vector3 flippedX = transform.localScale;
        flippedX.x = -flippedX.x;
        transform.localScale = flippedX;
    }
}
