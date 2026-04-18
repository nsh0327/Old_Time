using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Door : TimeFieldObject
{
    private bool _isOpen;
    [SerializeField] private bool _requirButton;
    [SerializeField] private Button _linkedButton;
    private Animator _animator;

    private void Start()
    {
        Debug.Log("Door Ω∫≈©∏≥∆Æ Ω√¿€µ ");
    }
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    protected override void Activate()
    {
        Debug.Log("Door Activate »£√‚µ ");
        base.Activate();
        if(!_requirButton)
        {
            Open();
        }
        else if(_requirButton && _linkedButton.IsPressed)
        {
            Open();
        }
        
    }
    protected override void Deactivate()
    {
        base.Deactivate();
        if (!_isOpen)
            Close();
        
    }

    public void Open()
    {
        if(_animator != null)
            _animator.SetTrigger("Open");
        _isOpen = true;
        Debug.Log("Door Open");
    }

    public void Close()
    {
        if (_animator != null)
            _animator.SetTrigger("Close");
        _isOpen = false;
        Debug.Log("Door Close");
    }

}
