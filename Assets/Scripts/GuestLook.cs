using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestLook : Guest
{
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
            if (!CanGuestSeeConveyance(c)) continue; //if guest cannot(!) see conveyance, we skip

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

    public bool CanGuestSeeConveyance(Conveyance conveyance)
    {
        Vector3 guestPosition = transform.position; //mouse position on the screen
        Vector3 startPosition = conveyance.StartPosition(guestPosition, this);
        RaycastHit hit;
        Debug.DrawLine(guestPosition, startPosition, Color.red);
        Debug.Break();
        if (!Physics.Linecast(guestPosition, startPosition, out hit)) { return true; } //if nothing is hit we can assume it is scene
        Debug.Log(conveyance.name + ": " + hit.transform.name + ": " + hit.transform.gameObject.tag);
        Debug.DrawLine(hit.point, guestPosition, Color.cyan);
        if (hit.transform.gameObject.tag != "Conveyance") return false; //if the thing hit was not a conveyance
        return true; //if nothing or conveyance was hit
    }
}