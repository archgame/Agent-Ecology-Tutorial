using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyanceInteract : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ToggleConveyanceActivation();
        }
    }

    private void ToggleConveyanceActivation()
    {
        Vector3 screenPoint = Input.mousePosition; //mouse position on the screen
        Ray ray = Camera.main.ScreenPointToRay(screenPoint); //converting the mouse position to ray from mouse position
        RaycastHit hit;
        if (!Physics.Raycast(ray.origin, ray.direction * 1000, out hit)) return; //was something hit?
        Debug.Log(hit.transform.gameObject.name);
        if (!hit.transform.gameObject.GetComponentInParent<Conveyance>()) return; //was hit on the layer?

        //if a layer was hit, set the camera follow and lookat
        Conveyance conveyance = hit.transform.gameObject.GetComponentInParent<Conveyance>();
        //if IsActive is public, you can do this, conveyance.IsActive = !conveyance.IsActive;
        if (conveyance.IsConveyanceActive())
        {
            conveyance.Deactivate();
        }
        else
        {
            conveyance.Activate();
        }
    }
}