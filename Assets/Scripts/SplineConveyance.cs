using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SplineInterpolator))]
public class SplineConveyance : Conveyance
{
    public float Testing = 0;
    private SplineInterpolator _mSplineInterp;
    private Dictionary<Guest, float> _guests = new Dictionary<Guest, float>();
    private float _step = 0;
    private float _period = 0.05f;

    private void OnDrawGizmos()
    {
        if (Path.Length < 2) return;
        if (Array.Exists(Path, go => go == null)) return;

        SplineInterpolator spline = SetupSpline(Path);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(spline.GetHermiteAtTime(Testing), Vector3.up);

        Gizmos.color = Color.white;
        Vector3 lastPosition = spline.GetHermiteAtTime(0);
        int nodeCount = spline.GetNodeCount();
        if (nodeCount % 2 != 0) { nodeCount--; } //test if the node count is even, if odd subtract one
        for (float t = _period; t <= nodeCount; t += _period)
        {
            Vector3 curretPosition = spline.GetHermiteAtTime(t);
            Gizmos.DrawLine(lastPosition, curretPosition);
            lastPosition = curretPosition;
        }
    }

    private float SplineLength(SplineInterpolator spline, float period)
    {
        float length = 0;
        Vector3 lastPosition = spline.GetHermiteAtTime(0);
        int nodeCount = spline.GetNodeCount();
        if (nodeCount % 2 != 0) { nodeCount--; } //test if the node count is even, if odd subtract one
        for (float t = _period; t <= nodeCount; t += _period)
        {
            Vector3 curretPosition = spline.GetHermiteAtTime(t);
            float distance = Vector3.Distance(lastPosition, curretPosition);
            length += distance;
            lastPosition = curretPosition;
        }
        return length;
    }

    private SplineInterpolator SetupSpline(IEnumerable<GameObject> gos)
    {
        //converting gameobjects to a list of transforms
        List<Transform> transforms = new List<Transform>();
        foreach (GameObject go in gos)
        {
            transforms.Add(go.transform);
        }

        //setup spline
        SplineInterpolator interp = transform.GetComponent<SplineInterpolator>();
        interp.Reset();
        for (int c = 0; c < transforms.Count; c++)
        {
            interp.AddPoint(transforms[c].position, transforms[c].rotation, c, new Vector2(0, 1));
        }
        interp.StartInterpolation(null, false, eWrapMode.ONCE);
        return interp;
    }

    // Start is called before the first frame update
    private void Start()
    {
        SetDestination();
        _mSplineInterp = SetupSpline(Path);
    }

    // Update is called once per frame
    public override void ConveyanceUpdate(Guest guest)
    {
        if (Path.Length < 2) return;

        //add guest to dictionary
        if (!_guests.ContainsKey(guest))
        {
            _guests.Add(guest, 0);
            guest.transform.position = Path[0].transform.position;
            return;
        }

        //move guest along
        _guests[guest] += Time.deltaTime * Speed;
        Vector3 position = _mSplineInterp.GetHermiteAtTime(_guests[guest]); //+++
        guest.transform.forward = position - guest.transform.position; //make sure guest is facing movement direction
        guest.transform.position = position;

        //once we reach end, remove the guest
        int nodeCount = _mSplineInterp.GetNodeCount(); //+++
        if (nodeCount % 2 != 0) { nodeCount--; } //+++
        if (_guests[guest] >= nodeCount)
        {
            _guests.Remove(guest);
            guest.NextDestination();
        }
    }
}