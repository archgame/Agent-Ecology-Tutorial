using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCamera FollowCamera;
    private List<CinemachineVirtualCamera> _cameras;
    private int _currentIndex = 0;

    // Start is called before the first frame update
    private void Start()
    {
        _cameras = FindObjectsOfType<CinemachineVirtualCamera>().ToList();
        _cameras.Remove(FollowCamera);

        UpdateCamera(_cameras, _currentIndex);
        FollowCamera.Priority = 9;
    }

    private void UpdateCamera(List<CinemachineVirtualCamera> cameras, int index)
    {
        if (FollowCamera.Priority != 9) { FollowCamera.Priority = 9; }
        foreach (CinemachineVirtualCamera camera in cameras)
        {
            camera.Priority = 9;
        }
        cameras[index].Priority = 11;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_cameras.Count < 2) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _currentIndex--;
            if (_currentIndex < 0) { _currentIndex = _cameras.Count - 1; }
            UpdateCamera(_cameras, _currentIndex);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _currentIndex++;
            if (_currentIndex >= _cameras.Count) { _currentIndex = 0; }
            UpdateCamera(_cameras, _currentIndex);
        }

        if (Input.GetMouseButton(0))
        {
            SetCameraTarget(FollowCamera, "Guest");
            FollowCamera.Priority = 12;
        }

        //when a guest leaves, the follow camera turns off
        if (FollowCamera.Priority != 9 && FollowCamera.Follow == null) { FollowCamera.Priority = 9; }
    }

    private void SetCameraTarget(CinemachineVirtualCamera camera, string layer)
    {
        Vector3 screenPoint = Input.mousePosition; //mouse position on the screen
        Ray ray = Camera.main.ScreenPointToRay(screenPoint); //converting the mouse position to ray from mouse position
        RaycastHit hit;
        if (!Physics.Raycast(ray.origin, ray.direction * 1000, out hit)) return; //was something hit?
        if (hit.transform.gameObject.layer != LayerMask.NameToLayer(layer)) return; //was hit on the layer?

        //if a layer was hit, set the camera follow and lookat
        camera.Follow = hit.transform;
        camera.LookAt = hit.transform;
    }
}