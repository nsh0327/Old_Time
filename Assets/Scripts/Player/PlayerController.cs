using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _airAcceleration = 25f;
    [SerializeField] private float _maxAirSpeed = 14f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 14f;
    [SerializeField] private float _maxJumpTime = 0.3f;
    [SerializeField] private float _jumpHoldForce = 8f;
    [SerializeField] private float _fallMultiplier = 2.5f;
    [SerializeField] private float _lowJumpMultiplier = 2f;
    [SerializeField] private float _coyoteTime = 0.15f;
    [SerializeField] private float _jumpBufferTime = 0.15f;
    [SerializeField] private float _runJumpBoostMultiplier = 1.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundedVelocityThreshold = 0.05f;

    [Header("State Threshold")]
    [SerializeField] private float _moveThreshold = 0.1f;
    [SerializeField] private float _fallThreshold = -0.05f;

    [Header("Hard Landing")]
    [SerializeField] private float _hardLandThreshold = 4f;
    [SerializeField] private float _hardLandDuration = 0.6f;

    public bool IsGrounded { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsFacingRight { get; private set; } = true;
    public bool IsJumping { get; private set; }
    public bool IsFalling { get; private set; }
    public bool IsHardLanding { get; private set; }
    public bool IsWallSliding { get; set; }
    public bool CanMove { get; set; } = true;
    public float MoveInput { get; private set; }
    public float VerticalVelocity { get; private set; }
    public float FallHeight { get; private set; }

    private Rigidbody2D _rigidbody2D;

    private bool _isJumpHeld;
    private bool _hasJumped;
    private bool _wasGrounded;

    private float _jumpTimeCounter;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private float _airPeakY;

    private Coroutine _hardLandCo;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _airPeakY = transform.position.y;
        CanMove = true;
        IsFacingRight = true;
    }

    private void Update()
    {
        GatherInput();
        CheckGrounded();
        UpdateTimers();
        HandleDirection();
        HandleJumpInput();
        TrackAirPeak();
        UpdateStateFlags();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJumpHoldForce();
        ApplyGravityModifier();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _hardLandCo = null;
    }

    public void SetFacingRight(bool value)
    {
        IsFacingRight = value;
    }

    private void GatherInput()
    {
        MoveInput = Input.GetAxisRaw("Horizontal");

        bool isRunKeyPressed = Input.GetKey(KeyCode.LeftShift);

        IsRunning =
            Mathf.Abs(MoveInput) > _moveThreshold &&
            isRunKeyPressed &&
            CanMove &&
            !IsHardLanding &&
            !IsWallSliding;

        _isJumpHeld = Input.GetButton("Jump");

        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
    }

    private void CheckGrounded()
    {
        _wasGrounded = IsGrounded;

        if (_groundCheck == null)
        {
            IsGrounded = false;
            return;
        }

        bool isTouchingGround = Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _groundLayer
        );

        bool canBeGrounded = _rigidbody2D.velocity.y <= _groundedVelocityThreshold;

        IsGrounded = isTouchingGround && canBeGrounded;

        if (!_wasGrounded && IsGrounded)
        {
            OnLanded();
        }

        if (IsGrounded && !_hasJumped)
        {
            _coyoteTimeCounter = _coyoteTime;
        }

        if (_wasGrounded && !IsGrounded)
        {
            _airPeakY = transform.position.y;
        }
    }

    private void UpdateTimers()
    {
        if (!IsGrounded)
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        _jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleDirection()
    {
        if (MoveInput > 0f)
        {
            IsFacingRight = true;
        }
        else if (MoveInput < 0f)
        {
            IsFacingRight = false;
        }
    }

    private void HandleJumpInput()
    {
        if (IsWallSliding)
        {
            return;
        }

        if (IsJumping)
        {
            _jumpTimeCounter += Time.deltaTime;

            if (!_isJumpHeld || _jumpTimeCounter >= _maxJumpTime || _rigidbody2D.velocity.y <= 0f)
            {
                IsJumping = false;
            }
        }

        bool canExecuteJump =
            _jumpBufferCounter > 0f &&
            _coyoteTimeCounter > 0f &&
            CanMove &&
            !IsHardLanding &&
            !_hasJumped;

        if (canExecuteJump)
        {
            ExecuteJump();
        }
    }

    private void ExecuteJump()
    {
        float horizontalVelocity = _rigidbody2D.velocity.x;

        bool isRunJump =
            IsRunning &&
            Mathf.Abs(MoveInput) > _moveThreshold &&
            Mathf.Abs(horizontalVelocity) > _moveThreshold &&
            Mathf.Sign(MoveInput) == Mathf.Sign(horizontalVelocity);

        if (isRunJump)
        {
            horizontalVelocity *= _runJumpBoostMultiplier;
            horizontalVelocity = Mathf.Clamp(horizontalVelocity, -_maxAirSpeed, _maxAirSpeed);
        }

        _rigidbody2D.velocity = new Vector2(horizontalVelocity, 0f);
        _rigidbody2D.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

        IsGrounded = false;
        IsJumping = true;
        _hasJumped = true;

        _jumpTimeCounter = 0f;
        _jumpBufferCounter = 0f;
        _coyoteTimeCounter = 0f;
        _airPeakY = transform.position.y;
        FallHeight = 0f;
    }

    private void HandleMovement()
    {
        if (IsWallSliding || !CanMove || IsHardLanding)
        {
            _rigidbody2D.velocity = new Vector2(0f, _rigidbody2D.velocity.y);
            return;
        }

        float targetX = MoveInput * (IsRunning ? _runSpeed : _walkSpeed);

        if (IsGrounded)
        {
            _rigidbody2D.velocity = new Vector2(targetX, _rigidbody2D.velocity.y);
            return;
        }

        float newX = Mathf.MoveTowards(
            _rigidbody2D.velocity.x,
            targetX,
            _airAcceleration * Time.fixedDeltaTime
        );

        newX = Mathf.Clamp(newX, -_maxAirSpeed, _maxAirSpeed);
        _rigidbody2D.velocity = new Vector2(newX, _rigidbody2D.velocity.y);
    }

    private void HandleJumpHoldForce()
    {
        bool canApplyHold =
            IsJumping &&
            _isJumpHeld &&
            _jumpTimeCounter < _maxJumpTime &&
            !IsWallSliding;

        if (!canApplyHold)
        {
            return;
        }

        _rigidbody2D.AddForce(Vector2.up * _jumpHoldForce, ForceMode2D.Force);
    }

    private void ApplyGravityModifier()
    {
        float baseGravity = Physics2D.gravity.y * _rigidbody2D.gravityScale;

        if (_rigidbody2D.velocity.y < 0f)
        {
            _rigidbody2D.velocity += Vector2.up * baseGravity * (_fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (_rigidbody2D.velocity.y > 0f && !_isJumpHeld)
        {
            _rigidbody2D.velocity += Vector2.up * baseGravity * (_lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private void TrackAirPeak()
    {
        if (!IsGrounded && transform.position.y > _airPeakY)
        {
            _airPeakY = transform.position.y;
        }
    }

    private void UpdateStateFlags()
    {
        VerticalVelocity = _rigidbody2D.velocity.y;
        IsFalling = !IsGrounded && VerticalVelocity < _fallThreshold;
    }

    private void OnLanded()
    {
        IsJumping = false;
        _hasJumped = false;

        FallHeight = Mathf.Max(0f, _airPeakY - transform.position.y);

        if (FallHeight >= _hardLandThreshold)
        {
            TriggerHardLanding();
        }
        else
        {
            FallHeight = 0f;
        }
    }

    private void TriggerHardLanding()
    {
        if (_hardLandCo != null)
        {
            StopCoroutine(_hardLandCo);
        }

        _hardLandCo = StartCoroutine(HardLandingCo());
    }

    private IEnumerator HardLandingCo()
    {
        IsHardLanding = true;
        CanMove = false;
        _rigidbody2D.velocity = Vector2.zero;

        yield return new WaitForSeconds(_hardLandDuration);

        IsHardLanding = false;
        CanMove = true;
        _hardLandCo = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
#endif
}