using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestTime : Guest
{
    public override void FindPath(ref Conveyance currentConveyance, ref List<Destination> destinations)
    {
        //Debug.Break();

        //get walking path distance
        Vector3 guestPosition = transform.position;
        Vector3 destinationPosition = Destination.transform.position;
        float distance = AgentWalkDistance(_agent, transform, guestPosition, destinationPosition, Color.yellow);
        float time = distance / _agent.speed; //+++

        //test all conveyances
        currentConveyance = null;
        Conveyance[] conveyances = GameObject.FindObjectsOfType<Conveyance>();
        foreach (Conveyance c in conveyances)
        {
            //guard statement,
            if (c.IsFull()) continue; //how many people are on the conveyance
            if (!c.IsConveyanceActive()) continue; //is conveyance active

            float distToC = AgentWalkDistance(_agent, transform, guestPosition, c.StartPosition(guestPosition, this), Color.green);
            float distC = c.WeightedTravelDistance(guestPosition, destinationPosition, this);
            float distFromC = AgentWalkDistance(_agent, transform, c.EndPosition(destinationPosition, this), destinationPosition, Color.red);

            float timeToC = distToC / _agent.speed; //+++
            float timeC = distC / c.Speed; //+++
            float timeFromC = distFromC / _agent.speed; //+++

            //Debug.DrawLine(guestPosition, c.StartPosition(), Color.black);
            Debug.DrawLine(c.StartPosition(guestPosition, this), c.EndPosition(destinationPosition, this), Color.cyan);
            //Debug.DrawLine(c.EndPosition(), destinationPosition, Color.white);

            if (time > timeToC + timeC + timeFromC) //+++
            {
                currentConveyance = c;
                time = timeToC + timeC + timeFromC; //+++
            }
        }

        //if there are no conveyances, we update the destination list with current destination
        if (currentConveyance == null)
        {
            destinations.Clear();
            destinations.Add(Destination);
            UpdateDestination();
            return;
        }

        //update destinations
        if (currentConveyance.GetType() == typeof(Vehicle))
        {
            Vehicle vehicle = _currentConveyance as Vehicle;
            vehicle.SetWaiting(this);
        }

        destinations.Clear();
        destinations.Add(currentConveyance.GetDestination(guestPosition, this));
        destinations.Add(Destination);
        Destination = destinations[0];
        UpdateDestination();
    }
}