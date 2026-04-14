using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsFacingRightHash = Animator.StringToHash("IsFacingRight");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");

    private Animator _animator;
    private PlayerController _controller;

   
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        UpdateAnimatorParameters();
    }


    private void UpdateAnimatorParameters()
    {
        _animator.SetFloat(SpeedHash, Mathf.Abs(_controller.MoveInput));
        _animator.SetBool(IsGroundedHash, _controller.IsGrounded);
        _animator.SetBool(IsFacingRightHash, _controller.IsFacingRight);
        _animator.SetFloat(VerticalVelocityHash, _controller.VerticalVelocity);
    }
}