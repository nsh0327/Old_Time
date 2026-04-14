using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeField : MonoBehaviour
{
    [Header("Field Settings")]
    [SerializeField] private float _fieldRadius = 3f;
    [SerializeField] private LayerMask _timeFieldLayer;
    [SerializeField] private float _checkInterval = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool _showFieldGizmo = true;

    private readonly List<TimeFieldObject> _objectsInField = new();
    private readonly List<TimeFieldObject> _previousObjects = new();

    private void Start()
    {
        StartCoroutine(CheckFieldCo());
    }

    private IEnumerator CheckFieldCo()
    {
        var waitInterval = new WaitForSeconds(_checkInterval);

        while (true)
        {
            UpdateFieldObjects();
            yield return waitInterval;
        }
    }

    private void UpdateFieldObjects()
    {
        _previousObjects.Clear();
        _previousObjects.AddRange(_objectsInField);
        _objectsInField.Clear();

        var hits = new Collider2D[20];
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            _fieldRadius,
            hits,
            _timeFieldLayer
        );

        for (int i = 0; i < count; i++)
        {
            if (hits[i].TryGetComponent<TimeFieldObject>(out var obj))
                _objectsInField.Add(obj);
        }

        // 범위 진입 → Activated
        foreach (var obj in _objectsInField)
        {
            if (!_previousObjects.Contains(obj))
                obj.EnterField();
        }

        // 범위 이탈 → Active
        foreach (var obj in _previousObjects)
        {
            if (!_objectsInField.Contains(obj))
                obj.ExitField();
        }
    }

    //등유 아이템으로 범위 확장
    public void ExpandField(float newRadius)
    {
        _fieldRadius = newRadius;
    }

    public float FieldRadius => _fieldRadius;

    //  디버그 기즈모 
    private void OnDrawGizmosSelected()
    {
        if (!_showFieldGizmo) return;
        Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.3f);
        Gizmos.DrawSphere(transform.position, _fieldRadius);
        Gizmos.color = new Color(1f, 0.9f, 0.3f, 1f);
        Gizmos.DrawWireSphere(transform.position, _fieldRadius);
    }
}