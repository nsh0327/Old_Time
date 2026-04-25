using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _damageFlashImage;

    [Header("Colors")]
    [SerializeField] private Color _highHealthColor = new Color(0.2f, 0.9f, 0.3f);
    [SerializeField] private Color _midHealthColor = new Color(1f, 0.85f, 0.1f);
    [SerializeField] private Color _lowHealthColor = new Color(0.9f, 0.15f, 0.15f);
    [SerializeField] private Color _flashColor = new Color(1f, 1f, 1f, 0.6f);

    [Header("Animation")]
    [SerializeField] private float _smoothSpeed = 8f;
    [SerializeField] private float _flashDuration = 0.15f;

    private float _targetFill;
    private float _flashTimer;

    private void Awake()
    {
        _targetFill = 1f;

        if (_fillImage != null)
        {
            _fillImage.fillAmount = 1f;
            _fillImage.color = _highHealthColor;
        }

        if (_damageFlashImage != null)
            _damageFlashImage.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged.AddListener(OnHealthChanged);
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged.RemoveListener(OnHealthChanged);
    }

    private void Update()
    {
        if (_fillImage == null) return;

        _fillImage.fillAmount = Mathf.Lerp(
            _fillImage.fillAmount,
            _targetFill,
            _smoothSpeed * Time.deltaTime
        );

        _fillImage.color = GetHealthColor(_fillImage.fillAmount);

        if (_damageFlashImage != null && _damageFlashImage.gameObject.activeSelf)
        {
            _flashTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(_flashTimer / _flashDuration);
            _damageFlashImage.color = new Color(
                _flashColor.r, _flashColor.g, _flashColor.b, alpha
            );

            if (_flashTimer <= 0f)
                _damageFlashImage.gameObject.SetActive(false);
        }
    }

    private void OnHealthChanged(float percent)
    {
        bool tookDamage = percent < _targetFill;
        _targetFill = percent;

        if (tookDamage && _damageFlashImage != null)
        {
            _damageFlashImage.gameObject.SetActive(true);
            _flashTimer = _flashDuration;
        }
    }

    private Color GetHealthColor(float percent)
    {
        if (percent > 0.5f)
            return Color.Lerp(_midHealthColor, _highHealthColor, (percent - 0.5f) * 2f);
        else
            return Color.Lerp(_lowHealthColor, _midHealthColor, percent * 2f);
    }
}