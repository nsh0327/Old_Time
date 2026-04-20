using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Collider2D))]
public class WallClimb : MonoBehaviour
{
    [Header("Wall Check")]
    [SerializeField] private Transform _wallCheckLeft;
    [SerializeField] private Transform _wallCheckRight;
    [SerializeField] private float _wallCheckRadius = 0.14f;
    [SerializeField] private float _autoWallCheckOffset = 0.03f;
    [SerializeField] private float _wallCheckVerticalOffset = 0f;
    [SerializeField] private LayerMask _wallLayer;

    [Header("Wall Hold")]
    [SerializeField] private float _wallHoldGravityScale = 0f;

    [Header("Wall Jump")]
    [SerializeField] private Vector2 _wallJumpForce = new Vector2(10f, 16f);
    [SerializeField] private float _wallJumpLockTime = 0.2f;
    [SerializeField] private float _sameWallBlockTime = 0.18f;

    private Rigidbody2D _rigidbody2D;
    private PlayerController _playerController;
    private Collider2D _bodyCollider;

    private float _defaultGravityScale;

    private bool _isTouchingLeftWall;
    private bool _isTouchingRightWall;
    private bool _isWallJumpLocked;

    private int _currentWallSide;
    private int _blockedWallSide;
    private int _wallJumpDirection;

    private float _sameWallBlockTimer;

    private Coroutine _wallJumpLockCo;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerController = GetComponent<PlayerController>();
        _bodyCollider = GetComponent<Collider2D>();
        _defaultGravityScale = _rigidbody2D.gravityScale;
    }

    private void Update()
    {
        UpdateSameWallBlockTimer();
        UpdateWallDetection();
        UpdateWallSlideState();
        HandleWallJumpInput();
        MaintainFacing();
    }

    private void FixedUpdate()
    {
        ApplyWallHold();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        _isWallJumpLocked = false;
        _currentWallSide = 0;
        _blockedWallSide = 0;
        _wallJumpDirection = 0;
        _sameWallBlockTimer = 0f;
        _wallJumpLockCo = null;

        if (_playerController != null)
        {
            _playerController.IsWallSliding = false;
            _playerController.CanMove = true;
        }

        if (_rigidbody2D != null)
        {
            _rigidbody2D.gravityScale = _defaultGravityScale;
        }
    }

    private void UpdateSameWallBlockTimer()
    {
        if (_sameWallBlockTimer <= 0f)
        {
            return;
        }

        _sameWallBlockTimer -= Time.deltaTime;

        if (_sameWallBlockTimer <= 0f)
        {
            _sameWallBlockTimer = 0f;
            _blockedWallSide = 0;
        }
    }

    private void UpdateWallDetection()
    {
        Vector2 leftCheckPosition = GetWallCheckPosition(false);
        Vector2 rightCheckPosition = GetWallCheckPosition(true);

        _isTouchingLeftWall = false;
        _isTouchingRightWall = false;

        if (_blockedWallSide != -1)
        {
            _isTouchingLeftWall = Physics2D.OverlapCircle(
                leftCheckPosition,
                _wallCheckRadius,
                _wallLayer
            );
        }

        if (_blockedWallSide != 1)
        {
            _isTouchingRightWall = Physics2D.OverlapCircle(
                rightCheckPosition,
                _wallCheckRadius,
                _wallLayer
            );
        }
    }

    private Vector2 GetWallCheckPosition(bool isRight)
    {
        if (isRight && _wallCheckRight != null)
        {
            return _wallCheckRight.position;
        }

        if (!isRight && _wallCheckLeft != null)
        {
            return _wallCheckLeft.position;
        }

        Bounds bounds = _bodyCollider.bounds;
        float x = isRight
            ? bounds.center.x + bounds.extents.x + _autoWallCheckOffset
            : bounds.center.x - bounds.extents.x - _autoWallCheckOffset;

        float y = bounds.center.y + _wallCheckVerticalOffset;
        return new Vector2(x, y);
    }

    private void UpdateWallSlideState()
    {
        if (_playerController == null)
        {
            return;
        }

        if (_isWallJumpLocked)
        {
            _playerController.IsWallSliding = false;
            return;
        }

        if (_playerController.IsGrounded || _playerController.IsHardLanding)
        {
            ReleaseWall();
            return;
        }

        bool isTouchingWall = _isTouchingLeftWall || _isTouchingRightWall;

        if (!isTouchingWall)
        {
            ReleaseWall();
            return;
        }

        _currentWallSide = ResolveWallSide();

        if (_currentWallSide == 0)
        {
            ReleaseWall();
            return;
        }

        _playerController.IsWallSliding = true;
        _playerController.CanMove = false;
        _rigidbody2D.gravityScale = _wallHoldGravityScale;
    }

    private int ResolveWallSide()
    {
        if (_isTouchingLeftWall && !_isTouchingRightWall)
        {
            return -1;
        }

        if (_isTouchingRightWall && !_isTouchingLeftWall)
        {
            return 1;
        }

        if (_playerController.MoveInput > 0f)
        {
            return 1;
        }

        if (_playerController.MoveInput < 0f)
        {
            return -1;
        }

        return _playerController.IsFacingRight ? 1 : -1;
    }

    private void ApplyWallHold()
    {
        if (_playerController == null || !_playerController.IsWallSliding)
        {
            return;
        }

        _rigidbody2D.velocity = Vector2.zero;
    }

    private void HandleWallJumpInput()
    {
        if (_playerController == null)
        {
            return;
        }

        if (!_playerController.IsWallSliding)
        {
            return;
        }

        if (!Input.GetButtonDown("Jump"))
        {
            return;
        }

        int jumpDirection = _currentWallSide == -1 ? 1 : -1;
        PerformWallJump(jumpDirection);
    }

    private void PerformWallJump(int jumpDirection)
    {
        int jumpedFromWallSide = _currentWallSide;

        _playerController.IsWallSliding = false;
        _playerController.CanMove = false;

        _rigidbody2D.gravityScale = _defaultGravityScale;

        _blockedWallSide = jumpedFromWallSide;
        _sameWallBlockTimer = _sameWallBlockTime;

        _wallJumpDirection = jumpDirection;
        _isWallJumpLocked = true;

        _playerController.SetFacingRight(_wallJumpDirection > 0);

        _rigidbody2D.velocity = new Vector2(
            _wallJumpDirection * _wallJumpForce.x,
            _wallJumpForce.y
        );

        if (_wallJumpLockCo != null)
        {
            StopCoroutine(_wallJumpLockCo);
        }

        _wallJumpLockCo = StartCoroutine(WallJumpLockCo());
    }

    private IEnumerator WallJumpLockCo()
    {
        float elapsedTime = 0f;

        while (elapsedTime < _wallJumpLockTime)
        {
            yield return new WaitForFixedUpdate();

            _rigidbody2D.velocity = new Vector2(
                _wallJumpDirection * _wallJumpForce.x,
                _rigidbody2D.velocity.y
            );

            elapsedTime += Time.fixedDeltaTime;
        }

        _playerController.CanMove = true;
        _isWallJumpLocked = false;
        _wallJumpDirection = 0;
        _wallJumpLockCo = null;
    }

    private void MaintainFacing()
    {
        if (_playerController == null)
        {
            return;
        }

        if (_playerController.IsWallSliding)
        {
            _playerController.SetFacingRight(_currentWallSide > 0);
        }
        else if (_isWallJumpLocked)
        {
            _playerController.SetFacingRight(_wallJumpDirection > 0);
        }
    }

    private void ReleaseWall()
    {
        _playerController.IsWallSliding = false;
        _playerController.CanMove = true;
        _currentWallSide = 0;
        _rigidbody2D.gravityScale = _defaultGravityScale;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector2 leftCheckPosition = _wallCheckLeft != null
            ? (Vector2)_wallCheckLeft.position
            : GetPreviewWallCheckPosition(false);

        Vector2 rightCheckPosition = _wallCheckRight != null
            ? (Vector2)_wallCheckRight.position
            : GetPreviewWallCheckPosition(true);

        Gizmos.DrawWireSphere(leftCheckPosition, _wallCheckRadius);
        Gizmos.DrawWireSphere(rightCheckPosition, _wallCheckRadius);
    }

    private Vector2 GetPreviewWallCheckPosition(bool isRight)
    {
        Collider2D collider2D = GetComponent<Collider2D>();

        if (collider2D == null)
        {
            return transform.position;
        }

        Bounds bounds = collider2D.bounds;
        float x = isRight
            ? bounds.center.x + bounds.extents.x + _autoWallCheckOffset
            : bounds.center.x - bounds.extents.x - _autoWallCheckOffset;

        float y = bounds.center.y + _wallCheckVerticalOffset;
        return new Vector2(x, y);
    }
#endif
}