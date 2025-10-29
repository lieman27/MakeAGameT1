using UnityEngine;

public static class PlayerAnimationParameters
{
    public const string IS_GROUNDED = "IsGrounded";
    public const string IS_MOVING = "IsMoving";
    public const string VELOCITY_X = "VelocityX";
    public const string VELOCITY_Y = "VelocityY";
    public const string IS_DASHING = "IsDashing";
    public const string IS_WALL_SLIDING = "IsWallSliding";
    public const string JUMP_TRIGGER = "Jump";
    public const string LAND_TRIGGER = "Land";
}

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;

    [Header("Animation Settings")]
    [SerializeField] private float movementThreshold = 0.1f;

    private bool wasGrounded;
    private bool wasInAir;

    void Reset()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponentInParent<PlayerController>();
    }

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError($"PlayerAnimationController on {gameObject.name} requires a PlayerController component!");
        }
    }

    void Start()
    {
        if (playerController != null)
        {
            wasGrounded = playerController.IsGrounded;
            wasInAir = !wasGrounded;
        }
    }

    void LateUpdate()
    {
        if (animator == null || playerController == null)
            return;

        UpdateAnimationParameters();
    }

    void UpdateAnimationParameters()
    {
        bool isGrounded = playerController.IsGrounded;
        Vector2 velocity = playerController.Velocity;
        bool isMoving = Mathf.Abs(velocity.x) > movementThreshold;
        bool isDashing = playerController.IsDashing;
        bool isWallSliding = playerController.IsWallSliding;

        animator.SetBool(PlayerAnimationParameters.IS_GROUNDED, isGrounded);
        animator.SetBool(PlayerAnimationParameters.IS_MOVING, isMoving);
        animator.SetFloat(PlayerAnimationParameters.VELOCITY_X, Mathf.Abs(velocity.x));
        animator.SetFloat(PlayerAnimationParameters.VELOCITY_Y, velocity.y);
        animator.SetBool(PlayerAnimationParameters.IS_DASHING, isDashing);
        animator.SetBool(PlayerAnimationParameters.IS_WALL_SLIDING, isWallSliding);

        HandleJumpAndLandTriggers(isGrounded, velocity.y);

        wasGrounded = isGrounded;
        wasInAir = !isGrounded;
    }

    void HandleJumpAndLandTriggers(bool isGrounded, float velocityY)
    {
        if (wasInAir && isGrounded)
        {
            animator.SetTrigger(PlayerAnimationParameters.LAND_TRIGGER);
        }

        if (wasGrounded && !isGrounded && velocityY > 0.1f)
        {
            animator.SetTrigger(PlayerAnimationParameters.JUMP_TRIGGER);
        }
    }

    public void PlayCustomAnimation(string stateName)
    {
        if (animator != null)
        {
            animator.Play(stateName);
        }
    }

    public void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
        }
    }
}
