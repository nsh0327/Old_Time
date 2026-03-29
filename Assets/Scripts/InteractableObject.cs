using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    //Move Waypoints Name
    public Transform[] waypoints; 
    //Where Count Waypoint
    private int currentWaypoint;
    private bool isActivated;
    public float moveSpeed;
    //isActivated Time
    public float duration;
    //Return Check
    private bool isReturning;


    void Start()
    {
        currentWaypoint = 0;
        isActivated = false;
        isReturning = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(isActivated)
        {
            Vector2 target = waypoints[currentWaypoint].position;

            transform.position = Vector2.Lerp(transform.position, target, moveSpeed * Time.fixedDeltaTime);

        }
        if(isReturning)
        {
            Vector2 returnTarget = waypoints[0].position;

            transform.position = Vector2.Lerp(transform.position, returnTarget, moveSpeed * Time.fixedDeltaTime);

            if (Vector2.Distance(transform.position, returnTarget) < 0.1f)
            {
                isReturning = false;
            }
        }
        

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Activate();
        }
    }
    void Activate()
    {
        isActivated = true;
        currentWaypoint = 1;

        StartCoroutine(DeactivateRoutine());
    }
    IEnumerator DeactivateRoutine()
    {
        yield return new WaitForSeconds(duration);
        isActivated = false;
        isReturning = true;
        currentWaypoint = 0;
    }
}
