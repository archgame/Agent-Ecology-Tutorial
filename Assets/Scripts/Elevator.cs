using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Elevator : Conveyance
{
    public GameObject Car;
    public GameObject Positions;
    public List<float> _buttonPressed = new List<float>();

    public enum State { MOVING, WAITING };

    public State CurrentState = State.WAITING;

    private Destination[] _destinations;
    private Dictionary<Guest, Vector3> _guests = new Dictionary<Guest, Vector3>(); //all guests

    //vvv guests that are in the car
    private Dictionary<GameObject, Guest> _positions = new Dictionary<GameObject, Guest>();

    private Dictionary<Guest, GameObject> _riders = new Dictionary<Guest, GameObject>();

    private float _maxWait = 1.0f;
    private float _waitTime = 0.0f;

    public override void SetDestination()
    {
        _waitTime = _maxWait;

        _destinations = GetComponentsInChildren<Destination>();

        //create the positions dictionary
        for (int i = 0; i < Positions.transform.childCount; i++)
        {
            _positions.Add(Positions.transform.GetChild(i).gameObject, null);
        }

        //set the occupnacy limit for each waiting lobby based on the number of positions in the elevator
        foreach (Destination destination in _destinations)
        {
            destination.OccupancyLimit = _positions.Count;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (_guests.Count == 0) return;
        if (_buttonPressed.Count == 0) return;

        //timer until car starts moving
        if (_waitTime <= 0)
        {
            CurrentState = State.MOVING;
        }
        else
        {
            _waitTime -= Time.deltaTime;
            return;
        }

        //parent all children in the car
        foreach (KeyValuePair<Guest, GameObject> kvp in _riders)
        {
            kvp.Key.transform.parent = Car.transform;
            //_riders[kvp.Key] = kvp.Value;
            //kvp.Value = GameObject
        }

        //move to next destination
        float nextY = _buttonPressed[0];
        Vector3 CarPosition = Car.transform.position;
        Vector3 NextPosition = new Vector3(CarPosition.x, nextY, CarPosition.z);
        Car.transform.position = Vector3.MoveTowards(CarPosition, NextPosition, Time.deltaTime * Speed);

        if (Vector3.Distance(CarPosition, NextPosition) > 0.01f) return;

        if (CurrentState == State.MOVING)
        {
            Car.transform.position = NextPosition;
            _buttonPressed.RemoveAt(0);
            _waitTime = _maxWait;
        }
        CurrentState = State.WAITING;
    }

    public override void ConveyanceUpdate(Guest guest)
    {
        //add guest to dictionary and their desired destination
        if (!_guests.ContainsKey(guest))
        {
            Destination destination = guest.GetUltimateDestination(); //getting the guest's final destination
            destination = GetDestination(destination.transform.position, guest); //converting into elevator stop floor
            _guests.Add(guest, destination.transform.position);

            //add button press
            if (!_buttonPressed.Contains(destination.transform.position.y))
            {
                _buttonPressed.Add(destination.transform.position.y);
                _buttonPressed.Sort();
            }
        }

        //guard statement if the elevator is moving
        if (CurrentState == State.MOVING) { return; }

        //call if the car if it isn't on the guest level
        if (Mathf.Abs(Car.transform.position.y - guest.transform.position.y) > 0.2f) //if Car and guest aren't on same level
        {
            Destination destination = GetDestination(guest.transform.position, guest);

            //add button press
            if (!_buttonPressed.Contains(destination.transform.position.y))
            {
                _buttonPressed.Add(destination.transform.position.y);
                _buttonPressed.Sort();
            }

            return;
        }

        //once we reach this point, we can assume the guest is either loading or unloading
        //we are assuming the guests that are unloading are children of the Car GameObject
        if (guest.transform.parent == Car.transform) //if a guest is inside (aka a child of) the Car
        {
            if (Car.transform.position.y != _guests[guest].y) return; //is the guest at the correct floor
            if (!UnloadingGuest(guest))
            {
                _waitTime = _maxWait; //if the guest isn't done unloading
            }
        }
        else
        {
            if (!LoadingGuest(guest))
            {
                _waitTime = _maxWait; //if the guest isn't done loading
            }
        }
    }

    public bool UnloadingGuest(Guest guest)
    {
        //at this point we assume the guest is unloading

        //switch out the point when begin the unloading process
        if (guest.transform.position == _riders[guest].transform.position)
        {
            Destination destination = GetDestination(Car.transform.position, guest);
            Vector3 offset = destination.transform.position - Car.transform.position;
            _guests[guest] = guest.transform.position + offset;
        }

        //unload the guest (animate the guest exiting
        guest.transform.position = Vector3.MoveTowards(guest.transform.position,
            _guests[guest],
            Time.deltaTime * Speed);

        //if the guest hasn't reached the disembark position, return false
        if (Vector3.Distance(guest.transform.position, _guests[guest]) > 0.01f) return false;

        //assume the guest has made it to the disembark position
        GameObject position = _riders[guest];
        _positions[position] = null;//this position is now open
        _riders.Remove(guest);
        _guests.Remove(guest);
        guest.transform.parent = null; //unparenting the guest from the car
        guest.NextDestination();
        return true;
    }

    public bool LoadingGuest(Guest guest)
    {
        if (!_riders.ContainsKey(guest))
        {
            //if the car is full, we can't add the new rider
            if (_riders.Count >= _positions.Count) return true;

            List<GameObject> gos = _positions.Keys.ToList();
            foreach (GameObject go in gos)
            {
                if (_positions[go] != null) continue;

                _positions[go] = guest;
                _riders.Add(guest, go);
                break;
            }
        }

        //guard statement if car is full
        if (!_riders.ContainsKey(guest)) return true;

        //load the guest (animate guest getting on the Car)
        guest.transform.position = Vector3.MoveTowards(guest.transform.position,
            _riders[guest].transform.position,
            Time.deltaTime * Speed);

        //if the guest hasn't reached the Car position, we indicate the loading is not finished
        if (Vector3.Distance(guest.transform.position, _riders[guest].transform.position) > 0.01f) return false;

        if (guest.Destination != null) { guest.Destination.RemoveGuest(guest); }
        return true;
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