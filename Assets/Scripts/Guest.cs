using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Guest : MonoBehaviour
{
    public enum Action { BATHING, WALKING, FOLLOWING }

    //public global variables
    public Destination Destination; //where the agent is going

    public float BathTime = 2.0f; //how long the agent stays in
    public Action Status; //our agent's current status

    //private global variables
    private float _bathTime = 0; //how long the agent has been in the bath

    private NavMeshAgent _agent; //our Nav Mesh Agent Component

    /// <summary>
    /// Called only once right after hitting Play
    /// </summary>
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        Status = Action.WALKING;
        UpdateDestination();
    }

    // Update is called once per frame
    public void GuestUpdate()
    {
        if (Status == Action.BATHING)
        {
            _bathTime += Time.deltaTime; //_bathTime = _bathTime + Time.deltaTime
            if (_bathTime > BathTime)
            {
                Status = Action.WALKING;
                _bathTime = 0; //++++
                Destination.RemoveGuest(this); //++++

                GameObject entrance = GameObject.Find("Entrance");
                Destination = entrance.GetComponent<Destination>();
                UpdateDestination();
            }
            //++++
            return; //so it doesn't run any code below
        }

        //guard statement
        if (Destination == null) return; //return stops the update here until next frame
        DestinationDistance(); //++++
    }

    private void DestinationDistance()
    {
        //test agent distance from destination
        if (Vector3.Distance(transform.position, Destination.transform.position) < 1.1f)
        {
            if (Destination.tag == "Bath")
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

    /// <summary>
    /// Start bath by changing agent status and stopping the agent
    /// </summary>
    private void StartBath()
    {
        Status = Action.BATHING;
        _agent.isStopped = true;
    }
}