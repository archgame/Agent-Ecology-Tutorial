using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Escalator : Conveyance
{
    private Dictionary<Guest, List<Vector3>> _guests = new Dictionary<Guest, List<Vector3>>();

    public override void ConveyanceUpdate(Guest guest)
    {
        if (!_guests.ContainsKey(guest)) //add guest to dictionary
        {
            List<Vector3> vecs = new List<Vector3>();
            foreach (GameObject go in Path)
            {
                vecs.Add(go.transform.position);
            }
            _guests.Add(guest, vecs);
        }

        guest.transform.position = Vector3.MoveTowards(
            guest.transform.position,
            _guests[guest][0],
            Time.deltaTime * Speed
            );

        if (Vector3.Distance(guest.transform.position, _guests[guest][0]) < 0.01)
        {
            _guests[guest].RemoveAt(0);

            if (_guests[guest].Count == 0)
            {
                _guests.Remove(guest);
                guest.NextDestination();
            }
        }
    }

    public override int NumberOfGuests()
    {
        return _guests.Count;
    }
}