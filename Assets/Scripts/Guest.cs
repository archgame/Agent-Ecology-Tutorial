﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Guest : MonoBehaviour
{
    public enum Action { BATHING, WALKING, FOLLOWING, RIDING }

    //public global variables
    public Destination Destination; //where the agent is going

    public int Baths = 3; //the number of baths our guest will take
    public float BathTime = 2.0f; //how long the agent stays in
    public Action Status; //our agent's current status

    //private global variables
    private float _bathTime = 0; //how long the agent has been in the bath

    private NavMeshAgent _agent; //our Nav Mesh Agent Component
    private Conveyance _currentConveyance = null;
    private List<Destination> _destinations = new List<Destination>();
    private Destination _tempDestination;

    /// <summary>
    /// Called only once right after hitting Play
    /// </summary>
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        Status = Action.WALKING;
        UpdateDestination();
        FindPath(ref _currentConveyance, ref _destinations);
    }

    // Update is called once per frame
    public void GuestUpdate()
    {
        if (Status == Action.RIDING)
        {
            _currentConveyance.ConveyanceUpdate(this);
        }
        if (Status == Action.BATHING)
        {
            _bathTime += Time.deltaTime; //_bathTime = _bathTime + Time.deltaTime
            if (_bathTime > BathTime)
            {
                _tempDestination = Destination;
                Destination = null;

                if (Baths == 0) //if guest is done with baths
                {
                    GameObject entrance = GameObject.Find("Entrance");
                    Destination = entrance.GetComponent<Destination>();
                }
                else //if guest needs new bath assigned
                {
                    GuestManager.Instance.AssignOpenBath(this); //Destination is assigned inside metho
                }
                if (Destination == null) return;

                //_tempDestination.RemoveGuest(this); //remove guest from current bath
                _destinations[0].RemoveGuest(this); //remove guest from current bath
                _destinations.RemoveAt(0); //remove current bath from destination list
                _bathTime = 0; //reseting bath time
                Status = Action.WALKING;  //start walking
                UpdateDestination(); //update new destination
                FindPath(ref _currentConveyance, ref _destinations); //finding best path
            }

            return; //so it doesn't run any code below
        }

        //guard statement
        if (Destination == null) return; //return stops the update here until next frame

        //orient gameobject direction
        if (_agent.enabled && _agent.velocity != Vector3.zero)
        {
            Vector3 forward = _agent.velocity;
            forward.y = 0;
            transform.forward = forward;
        }
        DestinationDistance(); //++++
    }

    private void DestinationDistance()
    {
        //test agent distance from destination
        if (Vector3.Distance(transform.position, Destination.transform.position) < 1.1f)
        {
            if (Destination.GetComponentInParent<Conveyance>())
            {
                Status = Action.RIDING;
                _agent.enabled = false;
                _currentConveyance = Destination.GetComponentInParent<Conveyance>();
                return;
            }
            else if (Destination.tag == "Bath")
            {
                StartBath();
                return;
            }
            else if (Destination.tag == "Entrance")
            {
                Destination.gameObject.GetComponent<GuestManager>().GuestExit(this);
                //GuestManager manager = Destination.gameObject.GetComponent<GuestManager>();
                //manager.GuestExit(this);
                return;
            }
        }
    }

    /// <summary>
    /// Update the agents destination and make sure the agent isn't stopped
    /// </summary>
    private void UpdateDestination()
    {
        _agent.SetDestination(Destination.transform.position);
        _agent.isStopped = false;
    }

    public void NextDestination()
    {
        _agent.enabled = true;
        _destinations.RemoveAt(0);
        Destination = _destinations[0];
        Status = Action.WALKING;
        FindPath(ref _currentConveyance, ref _destinations); //this allows multiple conveyances
    }

    public float AgentWalkDistance(Vector3 start, Vector3 end, Color color)
    {
        //move agent to the start position
        Vector3 initialPosition = transform.position;
        _agent.enabled = false;
        transform.position = start;//_agent.Move(start - initialPosition);
        _agent.enabled = true;

        //test to see if agent has path or not
        float distance = Mathf.Infinity;
        NavMeshPath navMeshPath = _agent.path;
        if (!_agent.CalculatePath(end, navMeshPath))
        {
            //reset agent to original position
            _agent.enabled = false;
            transform.position = initialPosition;//_agent.Move(initialPosition - start);
            _agent.enabled = true;
            return distance;
        }

        Vector3[] path = navMeshPath.corners;
        if (path.Length < 2 || Vector3.Distance(path[path.Length - 1], end) > 2)
        {
            //reset agent to original position
            _agent.enabled = false;
            transform.position = initialPosition;//_agent.Move(initialPosition - start);
            _agent.enabled = true;
            return distance;
        }

        //get walking path distance
        distance = 0;
        for (int i = 1; i < path.Length; i++)
        {
            distance += Vector3.Distance(path[i - 1], path[i]);
            Debug.DrawLine(path[i - 1], path[i], color); //visualizing the path, not necessary to return
        }

        //reset agent to original position
        _agent.enabled = false;
        transform.position = initialPosition;//_agent.Move(initialPosition - start);
        _agent.enabled = true;

        return distance;
    }

    public virtual void FindPath(ref Conveyance currentConveyance, ref List<Destination> destinations)
    {
        //Debug.Break();

        //get walking path distance
        Vector3 guestPosition = transform.position;
        Vector3 destinationPosition = Destination.transform.position;
        float distance = AgentWalkDistance(guestPosition, destinationPosition, Color.yellow);

        //test all conveyances
        currentConveyance = null;
        Conveyance[] conveyances = GameObject.FindObjectsOfType<Conveyance>();
        foreach (Conveyance c in conveyances)
        {
            float distToC = AgentWalkDistance(guestPosition, c.StartPosition(guestPosition.y), Color.green);
            float distC = c.WeightedTravelDistance(guestPosition.y, destinationPosition.y);
            float distFromC = AgentWalkDistance(c.EndPosition(destinationPosition.y), destinationPosition, Color.red);

            //Debug.DrawLine(guestPosition, c.StartPosition(), Color.black);
            Debug.DrawLine(c.StartPosition(guestPosition.y), c.EndPosition(destinationPosition.y), Color.cyan);
            //Debug.DrawLine(c.EndPosition(), destinationPosition, Color.white);

            if (distance > distToC + distC + distFromC)
            {
                currentConveyance = c;
                distance = distToC + distC + distFromC;
            }
        }

        //if there are no conveyances, we update the destination list with current destination
        if (_currentConveyance == null)
        {
            destinations.Clear();
            destinations.Add(Destination);
            UpdateDestination();
            return;
        }

        //update destinations
        destinations.Clear();
        destinations.Add(currentConveyance.GetDestination(guestPosition.y));
        destinations.Add(Destination);
        Destination = destinations[0];
        UpdateDestination();
    }

    /// <summary>
    /// Start bath by changing agent status and stopping the agent
    /// </summary>
    private void StartBath()
    {
        Baths--;
        Status = Action.BATHING;
        _agent.isStopped = true;
    }

    public Destination GetUltimateDestination()
    {
        if (_destinations.Count == 0) return null;
        return _destinations[_destinations.Count - 1];
    }
}