using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GuestManager : MonoBehaviour
{
    [HideInInspector]
    public static GuestManager Instance { get; private set; } //for singleton

    public GameObject GuestPrefab; //{get;set;}guest gameobject to be instantiated
    public GameObject EmployeePrefab;

    public float EntranceRate = 0.5f; //the rate at which guests will enter

    private List<Guest> _guest = new List<Guest>(); //list of guests
    private List<Guest> _employee = new List<Guest>(); //list of guests
    private List<Destination> _destinations = new List<Destination>(); //list of destinations
    private List<Guest> _exitedGuests = new List<Guest>(); //guests that will exit
    private GuestEntrance[] _guestEntrances;
    private EmployeeEntrance[] _employeeEntrances;

    private float _lastEntrance = 0; //time since last entrant
    private int _occupancyLimit = 0; //occupancy limit maximum

    private void Awake()
    {
        //Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        GameObject[] destinations = GameObject.FindGameObjectsWithTag("Bath");
        destinations = Shuffle(destinations);

        foreach (GameObject go in destinations)
        {
            Destination destination = go.GetComponent<Destination>(); //getting the destination script from game object
            _destinations.Add(destination); //adding the destination script to the list
            _occupancyLimit += destination.OccupancyLimit; //increasing the occupancy limit maximum
        }

        _guestEntrances = GameObject.FindObjectsOfType<GuestEntrance>();
        _employeeEntrances = GameObject.FindObjectsOfType<EmployeeEntrance>();

        AdmitGuest();
    }

    private GameObject[] Shuffle(GameObject[] objects)
    {
        GameObject tempGO;
        for (int i = 0; i < objects.Length; i++)
        {
            //Debug.Log("i: " + i);
            int rnd = Random.Range(0, objects.Length);
            tempGO = objects[rnd];
            objects[rnd] = objects[i];
            objects[i] = tempGO;
        }
        return objects;
    }

    private void AdmitGuest()
    {
        //guard statement, if bath house is full
        //if (_occupancyLimit <= _guest.Count) return;
        if (_guest.Count >= _occupancyLimit - 1) return;

        //instantiate guest
        GuestEntrance[] guestEntrances = _guestEntrances.Where(x => x.EntranceOpen).ToArray();
        int randomIndex = Random.Range(0, guestEntrances.Length);
        Vector3 position = guestEntrances[randomIndex].transform.position;
        GameObject guest = Instantiate(GuestPrefab, position, Quaternion.identity); //adding our gameobject to scene
        _guest.Add(guest.GetComponent<Guest>()); //adding our gameobject guest script to the guest list
        Guest guestScript = guest.GetComponent<Guest>();
        //List<Destination> visitedBaths = guestScript.VisitedBaths();
        AssignOpenBath(guestScript);
    }

    private void AdmitEmployee()
    {
        if (EmployeePrefab == null) return;
        if (_employeeEntrances.Length == 0) return;
        if (_guest.Count % 3 != 0) return;
        if (_guest.Count / 3 <= _employee.Count) return;

        //instantiate employee
        int randomIndex = Random.Range(0, _employeeEntrances.Length);
        Vector3 position = _employeeEntrances[randomIndex].transform.position;
        GameObject guest = Instantiate(EmployeePrefab, position, Quaternion.identity); //adding our gameobject to scene
        _employee.Add(guest.GetComponent<Guest>()); //adding our gameobject guest script to the guest list
        Guest guestScript = guest.GetComponent<Guest>();
        //List<Destination> visitedBaths = guestScript.VisitedBaths();
        AssignOpenBath(guestScript);
    }

    public virtual void AssignOpenBath(Guest guest, List<Destination> visited = null)
    {
        foreach (Destination bath in _destinations)
        {
            //if bath is full guard statement
            if (bath.IsFull()) continue; //continue goes to the next line

            //make sure bath hasn't already been visited
            if (visited != null)
            {
                if (visited.Contains(bath))
                {
                    continue;
                }
            }

            //assign destination;
            guest.Destination = bath;
            bath.AddGuest(guest);
            break;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        SetClickBath();
        EntranceOpen();

        //call guest update on each guest, the manager controls the guests
        foreach (Guest guest in _guest)
        {
            guest.GuestUpdate();
        }
        foreach (Guest guest in _employee)
        {
            guest.GuestUpdate();
        }

        if (_exitedGuests.Count >= 0) { ExitGuests(); }

        //admit guests after entrance rate
        if (EntranceRate <= _lastEntrance)
        {
            AdmitEmployee();
            AdmitGuest();
            _lastEntrance = 0;
            return;
        }
        else //if(EntranceRate > _lastEntrance)
        {
            _lastEntrance += Time.deltaTime;
        }
    }

    public virtual void ExitGuests()
    {
        //foreach(Guest guest in _exitedGuests)
        for (int i = 0; i < _exitedGuests.Count; i++)
        {
            Guest guest = _exitedGuests[i];
            if (_guest.Contains(guest))
            {
                _guest.Remove(guest);
            }
            if (_employee.Contains(guest))
            {
                _employee.Remove(guest);
            }

            Destroy(guest.gameObject);
        }
        _exitedGuests.Clear();
    }

    public virtual void GuestExit(Guest guest)
    {
        _exitedGuests.Add(guest);
    }

    public virtual List<Guest> GuestList()
    {
        return _guest;
    }

    public virtual List<Destination> DestinationList()
    {
        return _destinations;
    }

    public virtual Destination RandomEntrance(Guest guest)
    {
        GuestEntrance[] guestEntrances = _guestEntrances.Where(x => x.EntranceOpen).ToArray();
        string name = guest.name.Replace("(Clone)", "");
        int randomIndex = 0;
        if (name == GuestPrefab.name)
        {
            randomIndex = Random.Range(0, guestEntrances.Length);
            return guestEntrances[randomIndex];
        }

        randomIndex = Random.Range(0, _employeeEntrances.Length);
        return _employeeEntrances[randomIndex];
    }

    public void SetClickBath()
    {
        //guard statement if no moust button clicked
        if (!Input.GetKeyDown(KeyCode.B)) return;
        if (_guest.Count == 0) return;

        Vector3 screenPoint = Input.mousePosition; //mouse position on the screen
        Ray ray = Camera.main.ScreenPointToRay(screenPoint); //converting the mouse position to ray from mouse position
        RaycastHit hit;
        if (!Physics.Raycast(ray.origin, ray.direction, out hit)) return; //was something hit?
        if (hit.transform.gameObject.tag != "Bath") return; //was a bath hit
        if (!hit.transform.GetComponent<Destination>()) return; //does gameobject tagged bath have Destination script

        //Debug.Log("Bath Hit");
        Guest guest = _guest[_guest.Count - 1];
        if (guest.Status == Guest.Action.RIDING) return; //make sure the guest is not on a conveyance
        Destination bath = hit.transform.GetComponent<Destination>();

        //if bath is full guard statement
        if (bath.IsFull()) return; //continue goes to the next line

        //assign destination;
        bath.AddGuest(guest);
        guest.NextDestination(bath);

        if (guest.gameObject.GetComponent<TrailRenderer>()) return;
        guest.gameObject.AddComponent<TrailRenderer>();
        guest.gameObject.GetComponent<TrailRenderer>().time = 30;
    }

    public void EntranceOpen()
    {
        //guard statement if no moust button clicked
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 screenPoint = Input.mousePosition; //mouse position on the screen
        Ray ray = Camera.main.ScreenPointToRay(screenPoint); //converting the mouse position to ray from mouse position
        RaycastHit hit;
        if (!Physics.Raycast(ray.origin, ray.direction, out hit)) return; //was something hit?
        if (!hit.transform.GetComponent<GuestEntrance>()) return; //does gameobject tagged bath have Destination script

        //Debug.Log("Entrance Hit");
        GuestEntrance[] guestEntrances = _guestEntrances.Where(x => x.EntranceOpen).ToArray();

        //if this is the last open GuestEntrance and we are trying to close it, we don't let that happen
        if (hit.transform.GetComponent<GuestEntrance>().EntranceOpen && guestEntrances.Length == 1) return;

        hit.transform.GetComponent<GuestEntrance>().EntranceOpen = !hit.transform.GetComponent<GuestEntrance>().EntranceOpen;
    }
}