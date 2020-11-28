using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestShy : Guest
{
    public int OtherGuestMax = 3; //maximum number of other guests that can be using the conveyance

    public override void FindPath(ref Conveyance currentConveyance, ref List<Destination> destinations)
    {
        //Debug.Break();

        //get walking path distance
        Vector3 guestPosition = transform.position;
        Vector3 destinationPosition = Destination.transform.position;
        float distance = AgentWalkDistance(_agent, transform, guestPosition, destinationPosition, Color.yellow);

        //test all conveyances
        currentConveyance = null;
        Conveyance[] conveyances = GameObject.FindObjectsOfType<Conveyance>();
        foreach (Conveyance c in conveyances)
        {
            //guard statement,
            if (c.IsFull()) continue; //how many people are on the conveyance
            if (!c.IsConveyanceActive()) continue; //is conveyance active
            if (c.NumberOfGuests() > OtherGuestMax) continue; //if the conveyance has more than maximum guests, skip

            float distToC = AgentWalkDistance(_agent, transform, guestPosition, c.StartPosition(guestPosition, this), Color.green);
            float distC = c.WeightedTravelDistance(guestPosition, destinationPosition, this);
            float distFromC = AgentWalkDistance(_agent, transform, c.EndPosition(destinationPosition, this), destinationPosition, Color.red);

            //Debug.DrawLine(guestPosition, c.StartPosition(), Color.black);
            Debug.DrawLine(c.StartPosition(guestPosition, this), c.EndPosition(destinationPosition, this), Color.cyan);
            //Debug.DrawLine(c.EndPosition(), destinationPosition, Color.white);

            if (distance > distToC + distC + distFromC)
            {
                currentConveyance = c;
                distance = distToC + distC + distFromC;
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