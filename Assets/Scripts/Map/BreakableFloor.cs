using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableFloor : MonoBehaviour
{
    private Animator _animator;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<FallingObject>() != null)
        {
            Break();
        }
    }

    public void Break()
    {
        Debug.Log("Break »£√‚µ ");
        if (_animator != null)
            _animator.SetTrigger("Break");

        Destroy(gameObject, 1f);
    }

}
