using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
 
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _runSpeed = 8f;


    [Header("Jump")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _fallMultiplier = 2.5f;
    [SerializeField] private float _lowJumpMultiplier = 3.5f;
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _jumpBufferTime = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody2D _rb;


    public bool IsGrounded { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsFacingRight { get; private set; } = true;
    public float MoveInput { get; private set; }
    public float VerticalVelocity => _rb.velocity.y;


    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckGround();
        HandleMove();
        HandleJump();
        HandleFlip();
        UpdateCounters();
    }

    private void FixedUpdate()
    {
        ApplyBetterGravity();
    }

    private void CheckGround()
    {
        IsGrounded = Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _groundLayer
        );
    }

    private void HandleMove()
    {
        MoveInput = Input.GetAxisRaw("Horizontal");
        IsRunning = Input.GetKey(KeyCode.LeftShift);

        float speed = IsRunning ? _runSpeed : _walkSpeed;
        _rb.velocity = new Vector2(MoveInput * speed, _rb.velocity.y);
    }

    private void HandleJump()
    {
        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
        }
    }

    private void UpdateCounters()
    {
        if (IsGrounded)
            _coyoteTimeCounter = _coyoteTime;
        else
            _coyoteTimeCounter -= Time.deltaTime;

        // 점프 버퍼
        if (Input.GetButtonDown("Jump"))
            _jumpBufferCounter = _jumpBufferTime;
        else
            _jumpBufferCounter -= Time.deltaTime;
    }


    private void ApplyBetterGravity()
    {
        if (_rb.velocity.y < 0f)
        {
            _rb.velocity += Vector2.up * Physics2D.gravity.y
                            * (_fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (_rb.velocity.y > 0f && !Input.GetButton("Jump"))
        {
            _rb.velocity += Vector2.up * Physics2D.gravity.y
                            * (_lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }


    private void HandleFlip()
    {
        if (MoveInput > 0f && !IsFacingRight)
        {
            IsFacingRight = true;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (MoveInput < 0f && IsFacingRight)
        {
            IsFacingRight = false;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }
}