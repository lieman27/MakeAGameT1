using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public Transform spriteRoot;

    [Header("Collision")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public Vector2 groundCheckOffset = new Vector2(0f, -0.6f);
    public float groundCheckRadius = 0.12f;

    public Transform wallCheck;
    public Vector2 wallCheckOffset = new Vector2(0.5f, 0f);
    public Vector2 wallCheckSize = new Vector2(0.1f, 0.9f);

    [Header("Movement")]
    public float maxRunSpeed = 8f;
    public float acceleration = 80f;
    public float deceleration = 80f;
    public float airAcceleration = 40f;
    public float airDeceleration = 40f;
    public float friction = 0f; // optional ground friction

    [Header("Jumping")]
    public float jumpVelocity = 12f;
    public float gravityScale = 4f; // main gravity while falling
    public float fallGravityMultiplier = 1.8f; // multiplies gravity when falling
    public float lowJumpMultiplier = 3.5f; // stronger gravity when jump is cut

    [Header("Timing")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    [Header("Dash")]
    public bool allowDash = true;
    public float dashSpeed = 22f;
    public float dashTime = 0.14f;
    public float dashCooldown = 0.25f; // short cooldown
    public bool canAirDashOnlyOnce = true;

    [Header("Wall Mechanics")]
    public bool allowWallSlide = true;
    public float wallSlideSpeed = 2f;
    public float wallJumpHorizontal = 8f;
    public float wallJumpVertical = 12f;
    public float wallStickTime = 0.15f; // how long to 'stick' before sliding

    // Input System
    private PlayerControls controls;

    // Internals
    float horizontalInput;
    bool jumpPressed;
    bool jumpHeld;
    bool dashPressed;

    bool isGrounded;
    bool wasGrounded;
    bool isTouchingWall;
    int wallDir; // -1 left, 1 right

    float coyoteTimer;
    float jumpBufferTimer;

    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    bool hasDashedThisAir;

    float wallStickTimer;

    Vector2 savedVelocityBeforeDash;

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

        // Setup input controls
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        Vector2 move = controls.Player.Move.ReadValue<Vector2>();
        horizontalInput = move.x;

        jumpPressed = controls.Player.Jump.WasPressedThisFrame();
        jumpHeld = controls.Player.Jump.IsPressed();
        dashPressed = controls.Player.Dash.WasPressedThisFrame();

        // timers
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            hasDashedThisAir = false; // reset dash on ground
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpPressed)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        dashCooldownTimer -= Time.deltaTime;

        // Flip sprite based on input
        if (spriteRoot != null)
        {
            if (horizontalInput > 0.1f) spriteRoot.localScale = new Vector3(1, 1, 1);
            else if (horizontalInput < -0.1f) spriteRoot.localScale = new Vector3(-1, 1, 1);
        }
    }

    void FixedUpdate()
    {
        UpdateCollisionChecks();

        if (isDashing)
        {
            DashPhysicsStep();
            return;
        }

        // Horizontal movement
        HandleHorizontalMovement();

        // Vertical movement (gravity, jump)
        HandleGravityAndJumping();

        // Dash
        if (allowDash && dashPressed && dashCooldownTimer <= 0f)
        {
            TryStartDash();
        }

        // Wall slide & wall jump
        HandleWallSlidingAndJump();

        wasGrounded = isGrounded;
    }

    void UpdateCollisionChecks()
    {
        // Ground check
        Vector2 groundPos = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + groundCheckOffset;
        isGrounded = Physics2D.OverlapCircle(groundPos, groundCheckRadius, groundLayer);

        // Wall check (box overlap to one side)
        isTouchingWall =
            Physics2D.OverlapBox((Vector2)transform.position + wallCheckOffset, wallCheckSize, 0f, groundLayer) ||
            Physics2D.OverlapBox((Vector2)transform.position - wallCheckOffset, wallCheckSize, 0f, groundLayer);

        // Determine wall direction relative to player
        bool leftWall = Physics2D.OverlapBox((Vector2)transform.position - wallCheckOffset, wallCheckSize, 0f, groundLayer);
        bool rightWall = Physics2D.OverlapBox((Vector2)transform.position + wallCheckOffset, wallCheckSize, 0f, groundLayer);
        if (leftWall && !rightWall) wallDir = -1;
        else if (rightWall && !leftWall) wallDir = 1;
        else wallDir = 0;
    }

    void HandleHorizontalMovement()
    {
        float targetSpeed = horizontalInput * maxRunSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? (isGrounded ? acceleration : airAcceleration) : (isGrounded ? deceleration : airDeceleration);

        float velX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(velX, rb.linearVelocity.y);
    }

    void HandleGravityAndJumping()
    {
        // Gravity multipliers for better feel
        if (rb.linearVelocity.y < 0f) // falling
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !jumpHeld) // jump cut
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        // Can we jump? Check coyote time
        bool canUseCoyote = coyoteTimer > 0f;
        if (jumpBufferTimer > 0f && (canUseCoyote || isGrounded))
        {
            PerformJump();
            jumpBufferTimer = 0f;
        }
    }

    void PerformJump()
    {
        Vector2 vel = rb.linearVelocity;
        vel.y = jumpVelocity;
        rb.linearVelocity = vel;
        coyoteTimer = 0f;
    }

    void TryStartDash()
    {
        if (hasDashedThisAir && isGrounded) hasDashedThisAir = false;

        if (isGrounded || !hasDashedThisAir)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;
        savedVelocityBeforeDash = rb.linearVelocity;

        Vector2 dashDir = controls.Player.Move.ReadValue<Vector2>();
        if (dashDir.sqrMagnitude == 0f)
        {
            dashDir = spriteRoot != null && spriteRoot.localScale.x < 0 ? Vector2.left : Vector2.right;
        }
        dashDir.Normalize();
        rb.linearVelocity = dashDir * dashSpeed;

        if (!isGrounded) hasDashedThisAir = true;
    }

    void DashPhysicsStep()
    {
        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }

    void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -maxRunSpeed, maxRunSpeed), rb.linearVelocity.y);
    }

    void HandleWallSlidingAndJump()
    {
        if (!allowWallSlide) return;

        bool wallSliding = false;

        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0f)
        {
            wallSliding = true;
        }
        else
        {
            wallSliding = false;
            wallStickTimer = wallStuckMax;
        }

        if (wallSliding)
        {
            if (rb.linearVelocity.y < -wallSlideSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);

            if (jumpPressed)
            {
                Vector2 dir = new Vector2(-wallDir, 1f).normalized;
                rb.linearVelocity = new Vector2(dir.x * wallJumpHorizontal, dir.y * wallJumpVertical);
                wallStickTimer = 0f;
            }
        }
    }

    float wallStuckMax => wallStickTime;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 gPos = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireSphere(gPos, groundCheckRadius);

        Gizmos.color = Color.cyan;
        Vector2 wPos = wallCheck != null ? (Vector2)wallCheck.position : (Vector2)transform.position + wallCheckOffset;
        Gizmos.DrawWireCube(wPos, wallCheckSize);

        Vector2 wPos2 = wallCheck != null ? (Vector2)wallCheck.position : (Vector2)transform.position - wallCheckOffset;
        Gizmos.DrawWireCube(wPos2, wallCheckSize);
    }
}
