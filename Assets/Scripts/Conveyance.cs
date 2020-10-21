using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyance : MonoBehaviour
{
    public GameObject[] Path;
    public float Weight = 3.0f;
    public float Speed = 4.0f;

    private Destination _destination;
    private Dictionary<Guest, Vector3> _guests = new Dictionary<Guest, Vector3>();

    // Start is called before the first frame update
    private void Start()
    {
        SetDestination();
    }

    public void SetDestination()
    {
        _destination = GetComponentInChildren<Destination>();
    }

    // Update is called once per frame
    public virtual void ConveyanceUpdate(Guest guest)
    {
        if (!_guests.ContainsKey(guest)) //add guest to dictionary
        {
            _guests.Add(guest, Path[1].transform.position);
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

    public float WeightedTravelDistance()
    {
        float distance = 0;
        //guard statement
        if (Path.Length < 2) return distance;

        //getting the total path distance
        for (int i = 1; i < Path.Length; i++)
        {
            GameObject go1 = Path[i - 1];
            GameObject go2 = Path[i];

            float d = Vector3.Distance(go1.transform.position, go2.transform.position);
            distance += d;
        }

        //we scale the distance by the weight factor
        distance /= Weight; //distance = distance/Weight;
        return distance;
    }

    public Vector3 StartPosition()
    {
        if (Path.Length == 0) { return Vector3.zero; }
        return Path[0].transform.position;
    }

    public Vector3 EndPosition()
    {
        if (Path.Length == 0) { return Vector3.zero; }
        return Path[Path.Length - 1].transform.position;
        //GameObject go = Path[Path.Length - 1];
        //Transform t = go.transform;
        //Vector3 position = t.position;
        //return position;
        //doesn't go to this line
    }

    public Destination GetDestination()
    {
        return _destination;
    }
}