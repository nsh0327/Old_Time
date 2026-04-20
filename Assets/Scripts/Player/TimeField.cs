using System.Collections.Generic;
using UnityEngine;

public class TimeField : MonoBehaviour
{
    [Header("Field Settings")]
    [SerializeField] private float _fieldRadius = 3f;
    [SerializeField] private LayerMask _timeFieldLayer;
    [SerializeField] private int _maxDetectedObjects = 64;

    [Header("Debug")]
    [SerializeField] private bool _showFieldGizmo = true;

    private readonly HashSet<TimeFieldObject> _objectsInField = new HashSet<TimeFieldObject>();
    private readonly HashSet<TimeFieldObject> _previousObjects = new HashSet<TimeFieldObject>();

    private Collider2D[] _hitBuffer;
    private bool _isLocked;

    public float FieldRadius => _fieldRadius;

    private void Awake()
    {
        _hitBuffer = new Collider2D[_maxDetectedObjects];
    }

    private void OnEnable()
    {
        RefreshFieldObjects();
    }

    private void LateUpdate()
    {
        RefreshFieldObjects();
    }

    private void OnDisable()
    {
        ClearAllObjects();
    }

    public void ExpandField(float newRadius)
    {
        if (_isLocked)
        {
            return;
        }

        _fieldRadius = newRadius;

        Physics2D.SyncTransforms();
        RefreshFieldObjects();
    }

    public void LockField()
    {
        _isLocked = true;

        Physics2D.SyncTransforms();
        RefreshFieldObjects();
    }

    private void RefreshFieldObjects()
    {
        _previousObjects.Clear();

        foreach (TimeFieldObject obj in _objectsInField)
        {
            if (obj != null)
            {
                _previousObjects.Add(obj);
            }
        }

        _objectsInField.Clear();

        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            _fieldRadius,
            _hitBuffer,
            _timeFieldLayer
        );

        for (int i = 0; i < count; i++)
        {
<<<<<<< Updated upstream
            if (hits[i].TryGetComponent<TimeFieldObject>(out var obj))
=======
            Collider2D hit = _hitBuffer[i];

            if (hit == null)
            {
                continue;
            }

            TimeFieldObject obj = hit.GetComponent<TimeFieldObject>();

            if (obj != null)
            {
>>>>>>> Stashed changes
                _objectsInField.Add(obj);
            }
        }

        foreach (TimeFieldObject obj in _objectsInField)
        {
            if (!_previousObjects.Contains(obj))
<<<<<<< Updated upstream
=======
            {
>>>>>>> Stashed changes
                obj.EnterField();
            }
        }

        foreach (TimeFieldObject obj in _previousObjects)
        {
            if (!_objectsInField.Contains(obj))
            {
                obj.ExitField();
            }
        }
    }

    private void ClearAllObjects()
    {
        foreach (TimeFieldObject obj in _objectsInField)
        {
            if (obj != null)
            {
                obj.ExitField();
            }
        }

        _objectsInField.Clear();
        _previousObjects.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showFieldGizmo)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.3f);
        Gizmos.DrawSphere(transform.position, _fieldRadius);
        Gizmos.color = new Color(1f, 0.9f, 0.3f, 1f);
        Gizmos.DrawWireSphere(transform.position, _fieldRadius);
    }
}