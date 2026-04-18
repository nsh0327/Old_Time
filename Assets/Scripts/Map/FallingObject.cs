using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingObject : TimeFieldObject
{
    [SerializeField] private List<Transform> _waypoints;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private bool _destroyOnArrival;
    private int _currentIndex;
    private bool _isMoving;



    private void Update()
    {
        if(_isMoving)
        {
            if (_currentIndex >= _waypoints.Count) return;

            Vector2 direction = (_waypoints[_currentIndex].position - transform.position).normalized;
            transform.Translate(direction * _moveSpeed * Time.deltaTime);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.1f);
            if(hit.collider != null)
            {
                BreakableFloor floor = hit.collider.GetComponent<BreakableFloor>();
                if (floor != null)
                    floor.Break();
            }

            float distance = Vector2.Distance(transform.position, _waypoints[_currentIndex].position);
            if (distance < 0.1f)
            {
                _currentIndex++;
                if (_currentIndex >= _waypoints.Count)
                {
                    if(_destroyOnArrival)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        _isMoving = false;
                    }
        
                }
            }
        }
    }



    protected override void Activate()
    {
        base.Activate();
        _isMoving = true;

    }
    protected override void Deactivate()
    {
        base.Deactivate();
        _isMoving = false;
    
    }
}
