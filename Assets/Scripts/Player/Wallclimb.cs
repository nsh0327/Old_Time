using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Collider2D))]
public class WallClimb : MonoBehaviour
{
    [Header("Wall Detection")]
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _wallDetectDistance = 0.08f;
    [SerializeField] private float _wallDetectInset = 0.02f;

    [Header("Wall Hold")]
    [SerializeField] private float _wallHoldGravityScale = 0f;

    [Header("Wall Jump")]
    [SerializeField] private Vector2 _wallJumpForce = new Vector2(12f, 15f);
    [SerializeField] private float _wallJumpLockTime = 0.12f;
    [SerializeField] private float _sameWallBlockTime = 0.08f;

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
        _isTouchingLeftWall = _blockedWallSide != -1 && CheckWall(false);
        _isTouchingRightWall = _blockedWallSide != 1 && CheckWall(true);
    }

    private bool CheckWall(bool isRight)
    {
        Bounds bounds = _bodyCollider.bounds;

        float originX = isRight
            ? bounds.max.x - _wallDetectInset
            : bounds.min.x + _wallDetectInset;

        Vector2 origin = new Vector2(originX, bounds.center.y);
        Vector2 size = new Vector2(0.02f, bounds.size.y * 0.9f);
        Vector2 direction = isRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            size,
            0f,
            direction,
            _wallDetectDistance,
            _wallLayer
        );

        return hit.collider != null;
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
        if (_playerController == null || !_playerController.IsWallSliding)
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
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D == null)
        {
            return;
        }

        Bounds bounds = collider2D.bounds;

        Vector2 leftOrigin = new Vector2(bounds.min.x + _wallDetectInset, bounds.center.y);
        Vector2 rightOrigin = new Vector2(bounds.max.x - _wallDetectInset, bounds.center.y);
        Vector2 boxSize = new Vector2(0.02f, bounds.size.y * 0.9f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(leftOrigin + Vector2.left * _wallDetectDistance, boxSize);
        Gizmos.DrawWireCube(rightOrigin + Vector2.right * _wallDetectDistance, boxSize);
    }
#endif
}