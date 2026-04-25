using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingObject : TimeFieldObject
{

    [SerializeField] private float _gravityScale = 1f; //낙하 속도
    [SerializeField] private float _fallDelay = 0.5f; //떨어지기 전 대기 속도
    [SerializeField] private bool _destroyOnArrival;
    private bool _isFalling = false;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0;
        _rb.isKinematic = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if(_destroyOnArrival && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject, 1f);
        }
    }

    protected override void Activate()
    {
        base.Activate();
        Debug.Log("Activate 호출됨/ isFalling : "+_isFalling);
        if(!_isFalling)
            StartCoroutine(FallDelayCo());

    }
    protected override void Deactivate()
    {
        base.Deactivate();
        Debug.Log("Deactivate 호출됨/ isFalling : " + _isFalling);
        _isFalling = false;
        _rb.isKinematic = true;
        _rb.velocity = Vector2.zero;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;


    }

    private IEnumerator FallDelayCo()
    {
        Debug.Log("코루틴 시작");
        yield return new WaitForSeconds(_fallDelay);
        Debug.Log("중력 켜짐");
        _isFalling = true;
        _rb.isKinematic = false;
        _rb.gravityScale = _gravityScale;
        Debug.Log("중력 적용: " + _rb.gravityScale);
        _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }
}
