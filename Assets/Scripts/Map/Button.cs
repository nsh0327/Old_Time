using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : TimeFieldObject
{

    public bool IsPressed { get; private set; }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            IsPressed = false;
            Debug.Log("버튼 안눌림");
        
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            IsPressed = true;
            Debug.Log("버튼 눌림");
          
            
        }
    }


    protected override void Activate()
    {
        base.Activate();
     

    }

    protected override void Deactivate()
    {
        base.Deactivate();
     
    }
   


}
