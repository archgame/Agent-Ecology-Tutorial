using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshBaking : MonoBehaviour
{
    [HideInInspector]
    public static NavMeshBaking Instance { get; private set; } //for singleton

    private NavMeshSurface _surface;

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
        _surface = transform.GetComponent<NavMeshSurface>();
        BakeNavMesh();
    }

    //this can be called from other scripts using NavMeshBaking.Instance.BakeNavMesh();
    public void BakeNavMesh()
    {
        _surface.BuildNavMesh();
    }
}