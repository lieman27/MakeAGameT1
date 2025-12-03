using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform spriteRoot;

    [Header("Collision")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.6f);
    [SerializeField] private float groundCheckRadius = 0.12f;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.5f, 0f);
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.9f);

    [Header("Movement")]
    [SerializeField] public float maxRunSpeed = 8f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float deceleration = 80f;
    [SerializeField] private float airAcceleration = 40f;
    [SerializeField] private float airDeceleration = 40f;

    [Header("Jumping")]
    public float jumpVelocity = 12f;
    [SerializeField] private float gravityScale = 4f; // main gravity while falling
    [SerializeField] private float fallGravityMultiplier = 1.8f; // multiplies gravity when falling
    [SerializeField] private float lowJumpMultiplier = 3.5f; // stronger gravity when jump is cut

    [Header("Timing")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Dash")]
    [SerializeField] private bool allowDash = true;
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashTime = 0.14f;
    [SerializeField] private float dashCooldown = 0.25f; // short cooldown
    [SerializeField] private float dashBufferTime = 0.12f;

    [Header("Wall Mechanics")]
    [SerializeField] private bool allowWallSlide = true;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpHorizontal = 8f;
    [SerializeField] private float wallJumpVertical = 12f;
    [SerializeField] private float wallStickTime = 0.15f; // how long to 'stick' before sliding

    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;
    public bool IsWallSliding => isTouchingWall && !isGrounded && rb.linearVelocity.y < 0f;
    public Vector2 Velocity => rb.linearVelocity;

    // Input System
    private PlayerControls _controls;

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
    float dashBufferTimer;


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
        _controls = new PlayerControls();
    }

    void OnEnable()
    {
        _controls.Enable();
    }

    void OnDisable()
    {
        _controls.Disable();
    }

    void Update()
    {
        Vector2 move = _controls.Player.Move.ReadValue<Vector2>();
        horizontalInput = move.x;

        jumpPressed = _controls.Player.Jump.WasPressedThisFrame();
        jumpHeld = _controls.Player.Jump.IsPressed();
        dashPressed = _controls.Player.Dash.WasPressedThisFrame();

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            hasDashedThisAir = false;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpPressed)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (dashPressed)
            dashBufferTimer = dashBufferTime;
        else
            dashBufferTimer -= Time.deltaTime;

        dashCooldownTimer -= Time.deltaTime;

        if (spriteRoot != null)
        {
            if (horizontalInput > 0.1f)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (horizontalInput < -0.1f)
            {
                transform.localScale = new Vector3(-1, 1, 1);

            }
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

        HandleHorizontalMovement();
        HandleGravityAndJumping();

        if (allowDash && dashBufferTimer > 0f && dashCooldownTimer <= 0f)
        {
            TryStartDash();
        }

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
        dashBufferTimer = 0f;
        savedVelocityBeforeDash = rb.linearVelocity;

        Vector2 dashDir = _controls.Player.Move.ReadValue<Vector2>();
        if (dashDir.sqrMagnitude == 0f)
        {
            dashDir = spriteRoot != null && spriteRoot.localScale.x < 0 ? Vector2.left : Vector2.right;
        }
        dashDir.Normalize();
        rb.linearVelocity = dashDir * dashSpeed;
        rb.gravityScale = 0f;

        if (!isGrounded) hasDashedThisAir = true;
    }

    void DashPhysicsStep()
    {
        Vector2 dashDir = rb.linearVelocity.normalized;
        rb.linearVelocity = dashDir * dashSpeed;

        dashTimer -= Time.fixedDeltaTime;
        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }


    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = gravityScale;
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
            wallStickTimer = wallStickTime;
        }

        if (wallSliding)
        {
            if (rb.linearVelocity.y < -wallSlideSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }

        // Allow wall jump even when not actively sliding, just touching wall
        if (isTouchingWall && !isGrounded && jumpBufferTimer > 0f)
        {
            Vector2 dir = new Vector2(-wallDir, 1f).normalized;
            rb.linearVelocity = new Vector2(dir.x * wallJumpHorizontal, dir.y * wallJumpVertical);
            wallStickTimer = 0f;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
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
