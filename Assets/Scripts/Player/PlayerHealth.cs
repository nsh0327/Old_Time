using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 100f;

    [Header("TimeField Regen")]
    [SerializeField] private float _regenRateInField = 60f;

    [Header("Invincibility")]
    [SerializeField] private float _invincibilityDuration = 1.2f;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => _maxHealth;
    public float HealthPercent => CurrentHealth / _maxHealth;
    public bool IsDead => CurrentHealth <= 0f;

    public UnityEvent<float> OnHealthChanged = new UnityEvent<float>();
    public UnityEvent OnDied = new UnityEvent();

    private bool _isInTimeField;
    private bool _isInvincible;
    private float _invincibilityTimer;

    private void Awake()
    {
        CurrentHealth = _maxHealth;
    }

    private void Update()
    {
        if (_isInvincible)
        {
            _invincibilityTimer -= Time.deltaTime;
            if (_invincibilityTimer <= 0f)
                _isInvincible = false;
        }

        if (_isInTimeField && CurrentHealth < _maxHealth)
            ApplyHeal(_regenRateInField * Time.deltaTime);
    }

    public void TakeDamage(float percent)
    {
        if (_isInvincible || IsDead) return;

        float damage = _maxHealth * Mathf.Clamp01(percent);
        SetHealth(CurrentHealth - damage);

        _isInvincible = true;
        _invincibilityTimer = _invincibilityDuration;

        if (IsDead)
            OnDied?.Invoke();
    }

    public void ApplyHeal(float amount)
    {
        if (IsDead) return;
        SetHealth(CurrentHealth + amount);
    }

    public void SetInTimeField(bool value)
    {
        _isInTimeField = value;
    }

    private void SetHealth(float value)
    {
        CurrentHealth = Mathf.Clamp(value, 0f, _maxHealth);
        OnHealthChanged?.Invoke(HealthPercent);
    }
}