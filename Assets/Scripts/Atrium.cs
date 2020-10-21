using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atrium : MonoBehaviour
{
    public Material Main;
    public Material Alt;

    public void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<Guest>()) return;
        MeshRenderer mr = other.GetComponent<MeshRenderer>();
        mr.material = Alt;
    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<Guest>()) return;
        MeshRenderer mr = other.GetComponent<MeshRenderer>();
        mr.material = Main;
    }
}