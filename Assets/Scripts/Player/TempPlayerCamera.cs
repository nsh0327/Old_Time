using System.Collections;
using UnityEngine;

public class TempPlayerCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerController _player;

    [Header("Follow")]
    [SerializeField] private float _followSmooth = 6f;
    [SerializeField] private Vector2 _offset = new Vector2(0f, 1f);

    [Header("Look Ahead")]
    [SerializeField] private float _lookAheadX = 2.5f;
    [SerializeField] private float _lookAheadSmooth = 4f;

    [Header("Vertical Offset")]
    [SerializeField] private float _fallOffsetY = -1.5f;
    [SerializeField] private float _jumpOffsetY = 1f;
    [SerializeField] private float _verticalOffsetSmooth = 3f;

    [Header("Run Zoom")]
    [SerializeField] private float _defaultSize = 5f;
    [SerializeField] private float _runSize = 6f;
    [SerializeField] private float _zoomSmooth = 3f;

    [Header("Hard Landing Shake")]
    [SerializeField] private float _shakeDuration = 0.3f;
    [SerializeField] private float _shakeMagnitude = 0.18f;
    [SerializeField] private float _shakeDecay = 8f;

    [Header("TimeField Pulse")]
    [SerializeField] private float _pulseSize = 4.5f;
    [SerializeField] private float _pulseDuration = 0.2f;

    private Camera _camera;

    private Vector3 _currentVelocity;
    private float _currentLookAheadX;
    private float _currentVerticalOffset;
    private float _targetSize;

    private Vector3 _shakeOffset;
    private float _shakeTimer;
    private float _currentShakeMagnitude;

    private bool _wasHardLanding;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.orthographicSize = _defaultSize;
        _targetSize = _defaultSize;

        if (_player != null)
            transform.position = GetTargetPosition();
    }

    private void LateUpdate()
    {
        if (_player == null) return;

        UpdateLookAhead();
        UpdateVerticalOffset();
        UpdateZoom();
        UpdateShake();
        DetectHardLanding();

        Vector3 target = GetTargetPosition() + _shakeOffset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            target,
            ref _currentVelocity,
            1f / _followSmooth
        );

        _camera.orthographicSize = Mathf.Lerp(
            _camera.orthographicSize,
            _targetSize,
            _zoomSmooth * Time.deltaTime
        );
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 pos = _player.transform.position;
        pos.x += _offset.x + _currentLookAheadX;
        pos.y += _offset.y + _currentVerticalOffset;
        pos.z = transform.position.z;
        return pos;
    }

    private void UpdateLookAhead()
    {
        float targetLookAhead = _player.IsFacingRight ? _lookAheadX : -_lookAheadX;

        if (Mathf.Abs(_player.MoveInput) < 0.1f)
            targetLookAhead = 0f;

        _currentLookAheadX = Mathf.Lerp(
            _currentLookAheadX,
            targetLookAhead,
            _lookAheadSmooth * Time.deltaTime
        );
    }

    private void UpdateVerticalOffset()
    {
        float targetOffsetY = 0f;

        if (_player.IsFalling)
            targetOffsetY = _fallOffsetY;
        else if (_player.IsAscending)
            targetOffsetY = _jumpOffsetY;

        _currentVerticalOffset = Mathf.Lerp(
            _currentVerticalOffset,
            targetOffsetY,
            _verticalOffsetSmooth * Time.deltaTime
        );
    }

    private void UpdateZoom()
    {
        _targetSize = _player.IsRunning ? _runSize : _defaultSize;
    }

    private void DetectHardLanding()
    {
        bool isHardLanding = _player.IsHardLanding;

        if (isHardLanding && !_wasHardLanding)
            StartCoroutine(ShakeCo());

        _wasHardLanding = isHardLanding;
    }

    private void UpdateShake()
    {
        if (_shakeTimer > 0f)
        {
            _shakeTimer -= Time.deltaTime;
            float magnitude = _currentShakeMagnitude * (_shakeTimer / _shakeDuration);
            _shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * magnitude,
                Random.Range(-1f, 1f) * magnitude,
                0f
            );
        }
        else
        {
            _shakeOffset = Vector3.Lerp(_shakeOffset, Vector3.zero, _shakeDecay * Time.deltaTime);
        }
    }

    private IEnumerator ShakeCo()
    {
        _shakeTimer = _shakeDuration;
        _currentShakeMagnitude = _shakeMagnitude;
        yield return new WaitForSeconds(_shakeDuration);
    }

    public void TriggerTimeFieldPulse()
    {
        StartCoroutine(TimeFieldPulseCo());
    }

    private IEnumerator TimeFieldPulseCo()
    {
        _camera.orthographicSize = _pulseSize;

        float elapsed = 0f;
        while (elapsed < _pulseDuration)
        {
            elapsed += Time.deltaTime;
            _camera.orthographicSize = Mathf.Lerp(
                _pulseSize,
                _targetSize,
                elapsed / _pulseDuration
            );
            yield return null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_player == null) return;
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.4f);
        Gizmos.DrawLine(transform.position, _player.transform.position);
    }
#endif
}