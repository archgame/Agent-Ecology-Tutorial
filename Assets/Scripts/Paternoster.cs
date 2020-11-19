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

    private void Update()
    {
        for (int i = 0; i < Cars.transform.childCount; i++)
        {
            GameObject car = Cars.transform.GetChild(i).gameObject;

            //check if car is open
            if (_carRiders[car] == null)
            {
                float carDirection = _positions[_cars[car]].y - car.transform.position.y;

                foreach (KeyValuePair<Guest, Vector3> kvp in _guests)
                {
                    Guest guest = kvp.Key;

                    //guard statements
                    if (_riders.Contains(guest)) continue; //make sure guest doesn't move between cars
                    if (Mathf.Abs(car.transform.position.y - guest.transform.position.y) > 0.2f) continue;

                    //test guest direction
                    float guestDirection = kvp.Value.y - guest.transform.position.y;
                    if (!SameSign(carDirection, guestDirection)) continue; //continue to next guest

                    //load guest
                    _riders.Add(guest);
                    _carRiders[car] = guest;
                    IEnumerator coroutine = LoadPassenger(car, guest);
                    StartCoroutine(coroutine);
                    break; //don't check any more guests
                }
            }
            //check if guest has arrived at level
            else
            {
                Guest guest = _carRiders[car];
                Vector3 UnloadPosition = _guests[guest];
                if (Mathf.Abs(UnloadPosition.y - guest.transform.position.y) < 0.2f)
                {
                    //unload guest
                    _carRiders[car] = null;
                    IEnumerator coroutine = UnloadPassenger(car, guest);
                    StartCoroutine(coroutine);
                }
            }

            //animate cars
            //when the car reaches the position, we increase the index to the next position
            if (car.transform.position == _positions[_cars[car]])
            {
                int p = _cars[car] + 1;
                if (p >= _positions.Count) { p = 0; }
                _cars[car] = p;
            }

            //move car
            //int j = _cars[car];
            Vector3 newPos = Vector3.MoveTowards(car.transform.position,
                _positions[_cars[car]], //_positions[j]
                Speed * Time.deltaTime);
            car.transform.position = newPos;
        }
    }

    private IEnumerator LoadPassenger(GameObject car, Guest guest)
    {
        bool loading = true;
        while (loading)
        {
            guest.transform.position = Vector3.MoveTowards(guest.transform.position,
                car.transform.position,
                Time.deltaTime * Speed * 8);

            if (Vector3.Distance(guest.transform.position, car.transform.position) < 0.01f) { loading = false; }
            yield return new WaitForEndOfFrame();
        }

        guest.transform.parent = car.transform;
        yield break;
    }

    private IEnumerator UnloadPassenger(GameObject car, Guest guest)
    {
        bool unloading = true;
        while (unloading)
        {
            guest.transform.position = Vector3.MoveTowards(guest.transform.position,
                _guests[guest],
                Time.deltaTime * Speed * 8);

            if (Vector3.Distance(guest.transform.position, _guests[guest]) < 0.01f) { unloading = false; }
            yield return new WaitForEndOfFrame();
        }

        _riders.Remove(guest);
        _guests.Remove(guest);
        guest.transform.parent = null;
        guest.NextDestination();
        yield break;
    }

    public override void ConveyanceUpdate(Guest guest)
    {
        //guard statement if guest is already added
        if (_guests.ContainsKey(guest)) return;

        Destination destination = guest.GetUltimateDestination();
        destination = GetDestination(destination.transform.position, guest);
        _guests.Add(guest, destination.transform.position);
    }

    public override Destination GetDestination(Vector3 vec, Guest guest)
    {
        Destination[] tempDestinations = _destinations;
        tempDestinations = tempDestinations.OrderBy(go => Mathf.Abs(go.transform.position.y - vec.y)).ToArray();
        //tempDestinations = tempDestinations.OrderBy(x => x.name).ToArray();
        //tempDestinations = tempDestinations.OrderBy(x => Vector3.Distance(x.transform.position, Vector3.zero)).ToArray();
        return tempDestinations[0];
    }

    public override Vector3 StartPosition(Vector3 vec, Guest guest)
    {
        if (_destinations.Length == 0) { return Vector3.zero; }
        Destination destination = GetDestination(vec, guest);
        return destination.transform.position;
    }

    public override Vector3 EndPosition(Vector3 vec, Guest guest)
    {
        if (_destinations.Length == 0) { return Vector3.zero; }
        Destination destination = GetDestination(vec, guest);
        return destination.transform.position;
    }

    public override float WeightedTravelDistance(Vector3 start, Vector3 end, Guest guest)
    {
        float distance = 0;
        //guard statement
        if (_destinations.Length < 2) return distance;

        //get the total path distance
        Destination go1 = GetDestination(start, guest);
        Destination go2 = GetDestination(end, guest);
        distance = Vector3.Distance(go1.transform.position, go2.transform.position);

        //we scale the distance by the weight factor
        distance /= Weight;
        return distance;
    }
}