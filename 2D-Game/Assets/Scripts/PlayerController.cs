using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed, jumpSpeed, boostSpeed, boostForceMultiplier, boostForceMax;
    [SerializeField] private LayerMask ground;

    private PlayerControls playerControls;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;
    private SpriteRenderer renderer;
    private bool isWalkingLeft = false;
    private bool boostConfirmed = false;
    private float boostForce = 0f;
    private Vector2 boostOrigin;
    private Camera cam;
    private bool performBoost = false;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
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
        playerControls.Land.BoostConfirm.performed += BoostConfirmPerformed;
        playerControls.Land.BoostConfirm.canceled += BoostConfirmCancelled; ;
    }

    private void BoostConfirmCancelled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        boostConfirmed = false;
        boostOrigin = playerControls.Land.CursorPosition.ReadValue<Vector2>();
        performBoost = true;
    }

    private void BoostConfirmPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        boostConfirmed = true;
        Debug.Log("Boost Confirmed");
    }

    // Update is called once per frame
    void Update()
    {
        if (boostConfirmed == true)
        {
            boostForce += Time.deltaTime * boostForceMultiplier;
        }

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

    private void FixedUpdate()
    {
        if (performBoost)
        {
            Debug.Log("Performing Boost");
            Vector3 boostOrigin3D = new Vector3(boostOrigin.x, boostOrigin.y, 0f);
            Vector3 boostOriginWorld = cam.ScreenToWorldPoint(boostOrigin3D);
            Vector3 forceDirection3D = (transform.position - boostOriginWorld).normalized;
            Vector3 forceDirection2D = new Vector2(forceDirection3D.x, forceDirection3D.y);
            Debug.Log(forceDirection2D);
            Debug.Log("Boost Force " + boostForce);
            Debug.Log("Boost Speed " + boostSpeed);
            rb.AddForce(forceDirection2D * Mathf.Min(boostForce, boostForceMax) * boostSpeed, ForceMode2D.Impulse);

            performBoost = false;
            boostForce = 0f;
        }
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
