using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Paternoster : Conveyance
{
    public GameObject Cars;

    private Destination[] _destinations;
    private Dictionary<GameObject, int> _cars = new Dictionary<GameObject, int>();
    private List<Vector3> _positions = new List<Vector3>();
    private Dictionary<Guest, Vector3> _guests = new Dictionary<Guest, Vector3>();
    private Dictionary<GameObject, Guest> _carRiders = new Dictionary<GameObject, Guest>(); //keeps track of which cars have riders
    private List<Guest> _riders = new List<Guest>();

    public override void SetDestination()
    {
        _destinations = GetComponentsInChildren<Destination>();

        //create the positions dictionary
        for (int i = 0; i < Cars.transform.childCount; i++)
        {
            _cars.Add(Cars.transform.GetChild(i).gameObject, i);
            _positions.Add(Cars.transform.GetChild(i).transform.position);
            _carRiders.Add(Cars.transform.GetChild(i).gameObject, null);
        }

        //set the occupnacy limit for each waiting lobby
        foreach (Destination destination in _destinations)
        {
            destination.OccupancyLimit = 0;
        }
    }

    private bool SameSign(float x, float y)
    {
        return (x >= 0) ^ (y < 0);
    }

    public override Destination GetDestination(float y = 0)
    {
        Destination[] tempDestinations = _destinations;
        tempDestinations = tempDestinations.OrderBy(go => Mathf.Abs(go.transform.position.y - y)).ToArray();
        //tempDestinations = tempDestinations.OrderBy(x => x.name).ToArray();
        //tempDestinations = tempDestinations.OrderBy(x => Vector3.Distance(x.transform.position, Vector3.zero)).ToArray();
        return tempDestinations[0];
    }

    public override Vector3 StartPosition(float y = 0)
    {
        if (_destinations.Length == 0) { return Vector3.zero; }
        Destination destination = GetDestination(y);
        return destination.transform.position;
    }

    public override Vector3 EndPosition(float y = 0)
    {
        if (_destinations.Length == 0) { return Vector3.zero; }
        Destination destination = GetDestination(y);
        return destination.transform.position;
    }

    public override float WeightedTravelDistance(float startHeight = 0, float endHeight = 0)
    {
        float distance = 0;
        //guard statement
        if (_destinations.Length < 2) return distance;

        //get the total path distance
        Destination go1 = GetDestination(startHeight);
        Destination go2 = GetDestination(endHeight);
        distance = Vector3.Distance(go1.transform.position, go2.transform.position);

        //we scale the distance by the weight factor
        distance /= Weight;
        return distance;
    }
}