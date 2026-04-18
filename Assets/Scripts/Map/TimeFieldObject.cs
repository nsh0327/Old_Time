using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimeFieldObject : MonoBehaviour
{
    public bool IsInField { get; private set; }
    private bool isActivated;
    [SerializeField] private FieldReactionType _reactionType;

    public enum FieldReactionType
    {
        ActivateAndLatch,//1회만 실행
        ActiveInsiderField //매번 실행
    }

    public void EnterField()
    {
        IsInField = true;

        if(_reactionType == FieldReactionType.ActivateAndLatch)
        {
            Debug.Log("isActivated 상태 : " + isActivated);
            if(!isActivated)
            {
                Activate();
                isActivated = true;
            }
            
        }
        else
        {
            Activate();
        }
        
    }
    public void ExitField()
    {
        IsInField = false;
        Deactivate();
        
    }

    virtual protected void Activate()
    {

    }

    virtual protected void Deactivate()
    {

    }

}
