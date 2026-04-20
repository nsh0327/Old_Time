using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimator : MonoBehaviour
{
    private enum JumpType
    {
        Ground = 0,
        Run = 1
    }

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");

    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsFacingRightHash = Animator.StringToHash("IsFacingRight");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsFallingHash = Animator.StringToHash("IsFalling");
    private static readonly int IsWallSlidingHash = Animator.StringToHash("IsWallSliding");
    private static readonly int IsHardLandingHash = Animator.StringToHash("IsHardLanding");

    private static readonly int JumpTypeHash = Animator.StringToHash("JumpType");
    private static readonly int JumpTriggerHash = Animator.StringToHash("JumpTrigger");

    [Header("Animation Threshold")]
    [SerializeField] private float _moveThreshold = 0.1f;
    [SerializeField] private float _jumpStartVelocityThreshold = 0.05f;

    private Animator _animator;
    private PlayerController _playerController;

    private bool _wasGrounded;
    private bool _wasWallSliding;

    private int _groundedJumpType = (int)JumpType.Ground;
    private int _latchedJumpType = (int)JumpType.Ground;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerController = GetComponent<PlayerController>();

        _wasGrounded = _playerController.IsGrounded;
        _wasWallSliding = _playerController.IsWallSliding;
    }

    private void LateUpdate()
    {
        bool isGrounded = _playerController.IsGrounded;
        bool isWallSliding = _playerController.IsWallSliding;
        bool isHardLanding = _playerController.IsHardLanding;
        bool isFacingRight = _playerController.IsFacingRight;

        float moveSpeed = Mathf.Abs(_playerController.MoveInput);
        float verticalVelocity = _playerController.VerticalVelocity;

        bool isGroundRun =
            isGrounded &&
            _playerController.IsRunning &&
            moveSpeed > _moveThreshold;

        if (isGrounded)
        {
            _groundedJumpType = isGroundRun
                ? (int)JumpType.Run
                : (int)JumpType.Ground;
        }

        bool didStartGroundJump =
            _wasGrounded &&
            !isGrounded &&
            verticalVelocity > _jumpStartVelocityThreshold &&
            !isWallSliding;

        bool didStartWallJump =
            _wasWallSliding &&
            !isWallSliding &&
            verticalVelocity > _jumpStartVelocityThreshold;

        if (didStartGroundJump)
        {
            _latchedJumpType = _groundedJumpType;
            _animator.ResetTrigger(JumpTriggerHash);
            _animator.SetTrigger(JumpTriggerHash);
        }
        else if (didStartWallJump)
        {
            _latchedJumpType = (int)JumpType.Run;
            _animator.ResetTrigger(JumpTriggerHash);
            _animator.SetTrigger(JumpTriggerHash);
        }

        _animator.SetFloat(SpeedHash, moveSpeed);
        _animator.SetFloat(VerticalVelocityHash, verticalVelocity);

        _animator.SetBool(IsGroundedHash, isGrounded);
        _animator.SetBool(IsFacingRightHash, isFacingRight);
        _animator.SetBool(IsRunningHash, isGroundRun);
        _animator.SetBool(IsJumpingHash, _playerController.IsJumping);
        _animator.SetBool(IsFallingHash, _playerController.IsFalling);
        _animator.SetBool(IsWallSlidingHash, isWallSliding);
        _animator.SetBool(IsHardLandingHash, isHardLanding);

        _animator.SetInteger(JumpTypeHash, _latchedJumpType);

        _wasGrounded = isGrounded;
        _wasWallSliding = isWallSliding;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (_animator == null)
        {
            return;
        }

        _animator.ResetTrigger(JumpTriggerHash);

        _animator.SetFloat(SpeedHash, 0f);
        _animator.SetFloat(VerticalVelocityHash, 0f);

        _animator.SetBool(IsGroundedHash, false);
        _animator.SetBool(IsFacingRightHash, true);
        _animator.SetBool(IsRunningHash, false);
        _animator.SetBool(IsJumpingHash, false);
        _animator.SetBool(IsFallingHash, false);
        _animator.SetBool(IsWallSlidingHash, false);
        _animator.SetBool(IsHardLandingHash, false);

        _animator.SetInteger(JumpTypeHash, (int)JumpType.Ground);
    }
}