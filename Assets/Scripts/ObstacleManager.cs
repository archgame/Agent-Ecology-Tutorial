using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject ObstalcePrefab;
    public int ObstacleLimit;

    private List<GameObject> _obstacles = new List<GameObject>();

    // Update is called once per frame
    private void Update()
    {
        Vector3 position = Vector3.zero;
        if (ClickObject("Floor", ref position))
        {
            Debug.Log("Click Floor");
            //if we are at obstacle limit, remove oldest obstacle
            if (_obstacles.Count >= ObstacleLimit)
            {
                GameObject go = _obstacles[0];
                _obstacles.RemoveAt(0);
                Destroy(go);
            }

            //instantiate obstacle
            GameObject obstacle = Instantiate(ObstalcePrefab, position, Quaternion.identity); //adding our gameobject to scene
            _obstacles.Add(obstacle);
            return;
        }

        ClickRemoveObstacle();
    }

    private bool ClickObject(string layer, ref Vector3 vec)
    {
        Debug.Log("1");
        if (!Input.GetMouseButtonDown(0)) { return false; }
        Debug.Log("2");
        Vector3 screenPoint = Input.mousePosition; //mouse position on the screen
        Ray ray = Camera.main.ScreenPointToRay(screenPoint); //converting the mouse position to ray from mouse position
        RaycastHit hit;
        if (!Physics.Raycast(ray.origin, ray.direction, out hit)) return false; //was something hit?
        if (hit.transform.gameObject.layer != LayerMask.NameToLayer(layer)) return false; //was hit on the layer?

        //if a layer was hit, set the camera follow and lookat
        vec = hit.point;
        return true;
    }

    private void ClickRemoveObstacle()
    {
        if (!Input.GetMouseButtonDown(0)) { return; }

        Vector3 screenPoint = Input.mousePosition; //mouse position on the screen
        Ray ray = Camera.main.ScreenPointToRay(screenPoint); //converting the mouse position to ray from mouse position
        RaycastHit hit;
        if (!Physics.Raycast(ray.origin, ray.direction, out hit)) return; //was something hit?
        if (hit.transform.name.Replace("(Clone)", "") != ObstalcePrefab.name) return; //was hit on the layer?

        GameObject go = hit.transform.gameObject;
        if (_obstacles.Contains(go)) { _obstacles.Remove(go); }
        Destroy(go);
    }
}