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
        if (Status == Action.WAITING) return;
        if (Status == Action.WALKING)
        {
            //random walk scenario
            if (Path.Length == 0)
            {
                _timer += Time.deltaTime;
                if (_timer < _wanderTimer) return;

                Vector3 newPos = Guest.RandomNavSphere(transform.position, 100, -1);
                UpdateDestination(newPos);
                _timer = 0;//reset timer
                _wanderTimer = Random.Range(WanderTimer.x, WanderTimer.y);
                return;
            }
            //follow path scenario
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
            //if the vehicle is more than two vehicle widths away from destination return
            if (Vector3.Distance(transform.position, _guestDestination) > transform.localScale.x * 2) return;

            //after this the vehicle has arrived at the destination

            //unload agent
            _guest.transform.parent = null;
            _guest.NextDestination();
            _guest = null;

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

    public override void ConveyanceUpdate(Guest guest)
    {
        if (Status != Action.WAITING) return;

        if (Vector3.Distance(transform.position, guest.transform.position)
            > transform.localScale.x + guest.transform.localScale.x + 0.2f) return;

        Status = Action.RIDING;
        _agent.enabled = true;
        guest.transform.position = transform.position;
        guest.transform.parent = transform;
        _guestDestination = guest.GetUltimateDestination().transform.position;
        UpdateDestination(_guestDestination);
    }

    public void SetWaiting(Guest guest)
    {
        if (_guest == guest) return;
        _guest = guest;
        _agent.enabled = false;
        Status = Action.WAITING;
    }

    private void UpdateDestination(Vector3 position)
    {
        _agent.SetDestination(position);
        _agent.isStopped = false;
    }

    public override float WeightedTravelDistance(Vector3 start, Vector3 end, Guest guest)
    {
        Vector3 destination = _agent.destination;

        float toGuest = Guest.AgentWalkDistance(_agent, transform, transform.position, start, Color.green);
        float withGuest = Guest.AgentWalkDistance(_agent, transform, transform.position, end, Color.green);
        UpdateDestination(destination);

        if (toGuest == Mathf.Infinity || withGuest == Mathf.Infinity) return Mathf.Infinity;

        float distance = toGuest + withGuest;
        distance /= Weight;
        return distance;
    }

    public override Vector3 StartPosition(Vector3 vec, Guest guest)
    {
        return transform.position;
    }

    public override Vector3 EndPosition(Vector3 vec, Guest guest)
    {
        return vec; //assuming vehicle will take guest to final destination
    }

    public override Destination GetDestination(Vector3 vec, Guest guest)
    {
        return _dest;
    }

    public override bool IsFull()
    {
        if (_guest == null) return false;
        return true;
    }
}