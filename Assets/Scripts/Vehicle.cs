using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Destination))]
public class Vehicle : Conveyance
{
    public enum Action { WALKING, RIDING, WAITING, SEARCHING }

    public Action Status;
    private float _timer = 0;
    public Vector2 WanderTimer = new Vector2(2, 5);
    private float _wanderTimer = 2;
    private int _currentPathIndex = 0;
    private Vector3 _guestDestination = Vector3.zero;
    private Guest _guest = null;

    [HideInInspector]
    public NavMeshAgent _agent; //our Nav Mesh Agent Component

    private Destination _dest;

    public override void SetDestination()
    {
        _dest = GetComponent<Destination>();
        _agent = GetComponent<NavMeshAgent>();
        Status = Action.WALKING;
        Vector3 newPos = Guest.RandomNavSphere(transform.position, 100, -1);
        UpdateDestination(newPos);
        if (Path.Length == 0) return;

        UpdateDestination(Path[_currentPathIndex].transform.position);
    }

    // Update is called once per frame
    private void Update()
    {
        Debug.Log(Status);
        if (Status == Action.WAITING) return;
        //if vehicle is set to walking, we do this
        if (Status == Action.WALKING)
        {
            //if path isn't set, we randomly move the vehicle around
            if (Path.Length == 0)
            {
                _timer += Time.deltaTime;
                if (_timer < _wanderTimer) return;

                Vector3 newPos = Guest.RandomNavSphere(transform.position, 100, -1);
                UpdateDestination(newPos);
                _timer = 0;
                _wanderTimer = Random.Range(WanderTimer.x, WanderTimer.y);
                return;
            }
            //if path is set, we move along path
            else
            {
                if (Vector3.Distance(transform.position, Path[_currentPathIndex].transform.position) > 0.2f) return;

                _currentPathIndex++;
                if (_currentPathIndex >= Path.Length) { _currentPathIndex = 0; }
                UpdateDestination(Path[_currentPathIndex].transform.position);
            }
        }
        if (Status == Action.RIDING)
        {
            //check if vehicle is within range of destination
            if (Vector3.Distance(transform.position, _guestDestination) > transform.localScale.x * 2) return;

            //unload agent
            _guest.transform.parent = null;
            _guest.NextDestination();
            _guest = null;

            //reset vehicle
            Status = Action.WALKING;
            if (Path.Length == 0)
            {
                Vector3 newPos = Guest.RandomNavSphere(transform.position, 100, -1);
                UpdateDestination(newPos);
                _timer = 0;
                _wanderTimer = Random.Range(WanderTimer.x, WanderTimer.y);
            }
            else
            {
                UpdateDestination(Path[_currentPathIndex].transform.position);
            }
        }
    }

    public void SetWaiting(Guest guest)
    {
        if (_guest == guest) return;
        _guest = guest;
        _agent.enabled = false;
        Status = Action.WAITING;
    }
}