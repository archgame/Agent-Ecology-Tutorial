using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveConveyance : Conveyance
{
    private float _bezierPeriod = 0.05f;
    private float _step = 0;
    private Dictionary<Guest, float> _guests = new Dictionary<Guest, float>();

    public override void ConveyanceUpdate(Guest guest)
    {
        if (Path.Length < 4) return;

        //add guest to dictionary
        if (!_guests.ContainsKey(guest))
        {
            _guests.Add(guest, 0);
            _step = Speed / BezierLength();
            guest.transform.position = Path[0].transform.position;
            return;
        }

        //move guest along the curve
        _guests[guest] += Time.deltaTime * _step;
        Vector3 P0 = Path[0].transform.position;
        Vector3 P1 = Path[1].transform.position;
        Vector3 P2 = Path[2].transform.position;
        Vector3 P3 = Path[3].transform.position;
        Vector3 position = BezierPosition(_guests[guest], P0, P1, P2, P3);
        guest.transform.position = position;

        //remove guest once the end of curve is reached
        if (_guests[guest] >= 1)
        {
            _guests.Remove(guest);
            guest.NextDestination();
        }
    }

    private void OnDrawGizmos()
    {
        if (Path.Length < 4) return;

        Vector3 _gizmosPosition = Vector3.zero;
        Vector3 lastPosition = Path[0].transform.position;
        Vector3 P0 = Path[0].transform.position;
        Vector3 P1 = Path[1].transform.position;
        Vector3 P2 = Path[2].transform.position;
        Vector3 P3 = Path[3].transform.position;
        Gizmos.color = Color.white;

        for (float t = _bezierPeriod; t < 1; t += _bezierPeriod)
        {
            _gizmosPosition = BezierPosition(t, P0, P1, P2, P3);
            Gizmos.DrawLine(lastPosition, _gizmosPosition);
            lastPosition = _gizmosPosition;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(P0, P1);
        Gizmos.DrawLine(P2, P3);
    }

    private float BezierLength()
    {
        if (Path.Length < 4) return 0;

        Vector3 _gizmosPosition = Vector3.zero;
        Vector3 lastPosition = Path[0].transform.position;
        Vector3 P0 = Path[0].transform.position;
        Vector3 P1 = Path[1].transform.position;
        Vector3 P2 = Path[2].transform.position;
        Vector3 P3 = Path[3].transform.position;
        float length = 0;
        for (float t = _bezierPeriod; t < 1; t += _bezierPeriod)
        {
            _gizmosPosition = BezierPosition(t, P0, P1, P2, P3);
            float distance = Vector3.Distance(lastPosition, _gizmosPosition); //calc between two points
            lastPosition = _gizmosPosition;
            length += distance;
        }
        return length;
    }

    private Vector3 BezierPosition(float t,
           Vector3 P0, Vector3 P1, Vector3 P2, Vector3 P3)
    {
        return Mathf.Pow(1 - t, 3) * P0 +
            3 * Mathf.Pow(1 - t, 2) * t * P1 +
            3 * (1 - t) * Mathf.Pow(t, 2) * P2 +
            Mathf.Pow(t, 3) * P3;
    }
}