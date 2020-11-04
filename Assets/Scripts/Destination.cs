using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destination : MonoBehaviour
{
    public int OccupancyLimit = 1;

    private List<Guest> _occupants; //= new List<Guest>(); // list is private to protect from accidental methods

    //Awake happens before Start
    private void Awake()
    {
        _occupants = new List<Guest>();
    }

    public void AddGuest(Guest guest)
    {
        _occupants.Add(guest);
    }

    public void RemoveGuest(Guest guest)
    {
        _occupants.Remove(guest);
    }

    public bool IsFull()
    {
        if (OccupancyLimit == 0) return false; //if there is no occupancy limit, it is never full
        if (_occupants.Count >= OccupancyLimit) { return true; } //if the number of guests equals occupants, it is full
        return false;
    }

    public bool IsEmpty()
    {
        if (_occupants.Count == 0) { return true; }
        return false;
    }
}