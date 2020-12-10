using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Guest : MonoBehaviour
{
    [Header("UI")]
    public Text Text;

    public Slider Slider;

    public enum Action { BATHING, WALKING, FOLLOWING, RIDING, RANDOM }

    [Header("Destination")]
    //public global variables
    public Destination Destination; //where the agent is going

    public int Baths = 3; //the number of baths our guest will take
    public float BathTime = 2.0f; //how long the agent stays in
    public Action Status; //our agent's current status

    //private global variables
    private float _bathTime = 0; //how long the agent has been in the bath

    [HideInInspector]
    public NavMeshAgent _agent; //our Nav Mesh Agent Component

    [HideInInspector]
    public Conveyance _currentConveyance = null;

    public List<Destination> _destinations = new List<Destination>();
    public Destination _tempDestination;
    private List<Destination> _visitedBaths = new List<Destination>();
    private float _timer = 0;
    public Vector2 WanderTimer = new Vector2(2, 5);
    private float _wanderTimer = 2;

    /// <summary>
    /// Called only once right after hitting Play
    /// </summary>
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        //Status = Action.RANDOM;
        //Vector3 newPos = RandomNavSphere(transform.position, 100, -1);
        //UpdateDestination(newPos);

        Status = Action.WALKING;
        UpdateDestination();
        FindPath(ref _currentConveyance, ref _destinations);
    }

    // Update is called once per frame
    public virtual void GuestUpdate()
    {
        if (Status == Action.RANDOM)
        {
            _timer += Time.deltaTime;
            if (_timer >= _wanderTimer)
            {
                //*/
                List<Destination> baths = GuestManager.Instance.DestinationList();
                foreach (Destination bath in baths)
                {
                    float distance = Vector3.Distance(bath.transform.position, transform.position);
                    Debug.Log(distance);
                    if (distance > 15) continue;
                    GuestWalkDestination();
                    return;
                }
                //*/

                Vector3 newPos = RandomNavSphere(transform.position, 100, -1);
                UpdateDestination(newPos);
                _timer = 0;
                _wanderTimer = Random.Range(WanderTimer.x, WanderTimer.y);
                //Debug.Log("Wander Timer: " + _wanderTimer);
            }
            return;
        }
        if (Status == Action.RIDING)
        {
            _currentConveyance.ConveyanceUpdate(this);
        }
        if (Status == Action.BATHING)
        {
            _bathTime += Time.deltaTime; //_bathTime = _bathTime + Time.deltaTime
            if (_bathTime > BathTime)
            {
                _tempDestination = Destination;
                Destination = null;

                if (Baths == 0) //if guest is done with baths
                {
                    Destination = GuestManager.Instance.RandomEntrance(this);
                }
                else //if guest needs new bath assigned
                {
                    GuestManager.Instance.AssignOpenBath(this, _visitedBaths); //Destination is assigned inside metho
                }
                if (Destination == null) return;

                SetText("Walking");
                //_tempDestination.RemoveGuest(this); //remove guest from current bath
                _destinations[0].RemoveGuest(this); //remove guest from current bath
                _destinations.RemoveAt(0); //remove current bath from destination list
                _bathTime = 0; //reseting bath time
                Status = Action.WALKING;  //start walking
                UpdateDestination(); //update new destination
                FindPath(ref _currentConveyance, ref _destinations); //finding best path
            }

            return; //so it doesn't run any code below
        }

        //guard statement
        if (Destination == null) return; //return stops the update here until next frame

        //orient gameobject direction
        if (_agent.enabled && _agent.velocity != Vector3.zero)
        {
            Vector3 forward = _agent.velocity;
            forward.y = 0;
            transform.forward = forward;
        }
        DestinationDistance(); //++++
    }

    public virtual void GuestWalkDestination()
    {
        Status = Action.WALKING;
        UpdateDestination();
        FindPath(ref _currentConveyance, ref _destinations);
    }

    private void DestinationDistance()
    {
        //test agent distance from destination
        if (Vector3.Distance(transform.position, Destination.transform.position) < 1.1f)
        {
            if (Destination.GetComponentInParent<Conveyance>())
            {
                Status = Action.RIDING;
                _agent.enabled = false;
                _currentConveyance = Destination.GetComponentInParent<Conveyance>();
                return;
            }
            else if (Destination.tag == "Bath")
            {
                StartBath();
                return;
            }
            else if (Destination.tag == "Entrance")
            {
                Destination.gameObject.GetComponent<GuestManager>().GuestExit(this);
                //GuestManager manager = Destination.gameObject.GetComponent<GuestManager>();
                //manager.GuestExit(this);
                return;
            }
        }
    }

    /// <summary>
    /// Update the agents destination and make sure the agent isn't stopped
    /// </summary>
    public void UpdateDestination()
    {
        _agent.SetDestination(Destination.transform.position);
        _agent.isStopped = false;
    }

    private void UpdateDestination(Vector3 position)
    {
        _agent.SetDestination(position);
        _agent.isStopped = false;
    }

    public virtual void NextDestination(Destination destination = null)
    {
        _agent.enabled = true;
        _destinations.RemoveAt(0);
        if (destination == null)
        {
            Destination = _destinations[0];
        }
        else
        {
            _destinations.Clear();
            Destination = destination;
        }
        Status = Action.WALKING;
        FindPath(ref _currentConveyance, ref _destinations); //this allows multiple conveyances
    }

    public static float AgentWalkDistance(NavMeshAgent agent, Transform trans,
        Vector3 start, Vector3 end, Color color)
    {
        //in case they are the same position
        if (Vector3.Distance(start, end) < 0.01f) return 0;

        //move agent to the start position
        Vector3 initialPosition = trans.position;
        agent.enabled = false;
        trans.position = start;//_agent.Move(start - initialPosition);
        agent.enabled = true;

        //test to see if agent has path or not
        float distance = Mathf.Infinity;
        NavMeshPath navMeshPath = agent.path;
        if (!agent.CalculatePath(end, navMeshPath))
        {
            //reset agent to original position
            agent.enabled = false;
            trans.position = initialPosition;//_agent.Move(initialPosition - start);
            agent.enabled = true;
            return distance;
        }

        //check to see if there is a path
        Vector3[] path = navMeshPath.corners;
        if (path.Length < 2 || Vector3.Distance(path[path.Length - 1], end) > 2)
        {
            //reset agent to original position
            agent.enabled = false;
            trans.position = initialPosition;//_agent.Move(initialPosition - start);
            agent.enabled = true;
            return distance;
        }

        //get walking path distance
        distance = 0;
        for (int i = 1; i < path.Length; i++)
        {
            distance += Vector3.Distance(path[i - 1], path[i]);
            Debug.DrawLine(path[i - 1], path[i], color); //visualizing the path, not necessary to return
        }

        //reset agent to original position
        agent.enabled = false;
        trans.position = initialPosition;//_agent.Move(initialPosition - start);
        agent.enabled = true;

        return distance;
    }

    public virtual void FindPath(ref Conveyance currentConveyance, ref List<Destination> destinations)
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

            float distToC = AgentWalkDistance(_agent, transform, guestPosition, c.StartPosition(guestPosition, this), Color.green);
            float distC = c.WeightedTravelDistance(guestPosition, destinationPosition, this);
            float distFromC = AgentWalkDistance(_agent, transform, c.EndPosition(destinationPosition, this), destinationPosition, Color.red);

            //Debug.DrawLine(guestPosition, c.StartPosition(guestPosition, this), Color.black);
            Debug.DrawLine(c.StartPosition(guestPosition, this), c.EndPosition(destinationPosition, this), Color.cyan);
            //Debug.DrawLine(c.EndPosition(destinationPosition, this), destinationPosition, Color.white);

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

    /// <summary>
    /// Start bath by changing agent status and stopping the agent
    /// </summary>
    private void StartBath()
    {
        Baths--;
        _visitedBaths.Add(Destination);
        Status = Action.BATHING;
        _agent.isStopped = true;
        SetText("Bathing");
    }

    public virtual Destination GetUltimateDestination()
    {
        if (_destinations.Count == 0) return null;
        return _destinations[_destinations.Count - 1];
    }

    public virtual void SetText(string text)
    {
        if (Text == null) return;
        Text.text = text;
    }

    public virtual void SetSlider(float i)
    {
        if (Slider == null) return;
        Slider.value = i;
    }

    public virtual string GetText()
    {
        if (Slider == null) return string.Empty;
        return Text.text;
    }

    public virtual float GetSliderValue()
    {
        if (Slider == null) return Mathf.Infinity;
        return Slider.value;
    }

    public virtual List<Destination> VisitedBaths()
    {
        return _visitedBaths;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        //Debug.Break();
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        Debug.DrawLine(origin, randDirection, Color.blue);
        Debug.DrawRay(navHit.position, Vector3.up * 3, Color.cyan);
        return navHit.position;
    }
}