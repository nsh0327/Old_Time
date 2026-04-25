using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : TimeFieldObject
{
    [SerializeField] private float _moveSpeed;
    private PlayerController _playerController;
    private bool _isMoving;

    private void Update()
    {

        if(_isMoving)
        {
            Vector2 direction = (_playerController.transform.position - transform.position).normalized;
            transform.Translate(direction * _moveSpeed * Time.deltaTime);
        }
    }

    private void Awake()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
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
