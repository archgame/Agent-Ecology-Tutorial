using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoWay : Conveyance
{
    private Dictionary<Guest, Vector3> _guests = new Dictionary<Guest, Vector3>();

    public override void ConveyanceUpdate(Guest guest)
    {
        Debug.Log("CU");
        if (!_guests.ContainsKey(guest)) //add guest to dictionary
        {
            Vector3 guestPosition = guest.transform.position;
            Vector3 Path1 = Path[0].transform.position;
            Vector3 Path2 = Path[1].transform.position;
            float dist1 = Vector3.Distance(Path1, guestPosition);
            float dist2 = Vector3.Distance(Path2, guestPosition);
            if (dist1 > dist2)
            {
                Debug.Log("1");
                _guests.Add(guest, Path1);
            }
            else
            {
                Debug.Log("1");
                _guests.Add(guest, Path2);
            }
        }

        guest.transform.position = Vector3.MoveTowards(
            guest.transform.position,
            _guests[guest],
            Time.deltaTime * Speed
            );

        if (Vector3.Distance(guest.transform.position, _guests[guest]) < 0.01)
        {
            _guests.Remove(guest);
            guest.NextDestination();
        }
    }

    public override Vector3 StartPosition(Vector3 vec, Guest guest)
    {
        float dist = Mathf.Infinity;
        Vector3 startPosition = Vector3.zero;
        foreach (GameObject go in Path)
        {
            float distTemp = Guest.AgentWalkDistance(guest._agent, guest.transform, guest.transform.position, go.transform.position, Color.blue);
            Debug.DrawLine(guest.transform.position, go.transform.position, Color.red);
            if (dist < distTemp) continue; //if the distance is less than distance temp, then we continue to next Path

            dist = distTemp;
            startPosition = go.transform.position;
        }

        return startPosition;
    }

    public override Vector3 EndPosition(Vector3 vec, Guest guest)
    {
        float dist = Mathf.Infinity;
        Vector3 endPosition = Vector3.zero;
        foreach (GameObject go in Path)
        {
            float distTemp = Guest.AgentWalkDistance(guest._agent, guest.transform, go.transform.position, vec, Color.green);

            if (dist < distTemp) continue; //if the distance is less than distance temp, then we continue to next Path

            dist = distTemp;
            endPosition = go.transform.position;
        }

        return endPosition;
    }

    public override Destination GetDestination(Vector3 vec, Guest guest)
    {
        float dist = Mathf.Infinity;
        Destination destination = null;
        foreach (GameObject go in Path)
        {
            float distTemp = Guest.AgentWalkDistance(guest._agent, guest.transform, guest.transform.position, go.transform.position, Color.blue);

            if (dist < distTemp) continue; //if the distance is less than distance temp, then we continue to next Path
            if (!go.GetComponent<Destination>()) continue;

            dist = distTemp;
            destination = go.GetComponent<Destination>();
        }
        Debug.Log(destination.name);
        return destination;
    }
}