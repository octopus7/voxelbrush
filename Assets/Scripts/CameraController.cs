using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 dragStartPos = Vector3.zero;
    float cameraSpeed = 1.0f;
    Quaternion cameraOriginRotation = Quaternion.identity;

    void Update()
    {
        
    }

    public void MouseDown()
    {
        dragStartPos = Input.mousePosition;
        cameraOriginRotation = Camera.main.transform.rotation;
    }

    public void Drag()
    {
        var dragDelta = dragStartPos - Input.mousePosition;

        var euler = cameraOriginRotation.eulerAngles;

        euler.x = euler.x + dragDelta.y / Screen.width * 180;
        euler.y = euler.y - dragDelta.x / Screen.width * 180;

        Camera.main.transform.rotation = Quaternion.Euler(euler);

        float axisX = 0;
        float axisY = 0;
        float axisZ = 0;

        if (Input.GetKey(KeyCode.A)) axisX = -1;
        if (Input.GetKey(KeyCode.D)) axisX = 1;
        if (Input.GetKey(KeyCode.Q)) axisY = -1;
        if (Input.GetKey(KeyCode.E)) axisY = 1;
        if (Input.GetKey(KeyCode.S)) axisZ = -1;
        if (Input.GetKey(KeyCode.W)) axisZ = 1;

        KeepAcceleration();

        float delta = Time.deltaTime * cameraSpeed;

        if (axisX != 0 || axisY != 0 || axisZ != 0)
        {
            cameraSpeed += Time.deltaTime;
        }

        Camera.main.transform.Translate(Vector3.right * axisX * delta, Space.Self);
        Camera.main.transform.Translate(Vector3.up * axisY * delta, Space.Self);
        Camera.main.transform.Translate(Vector3.forward * axisZ * delta, Space.Self);
    }

    private void KeepAcceleration()
    {
        if (Input.GetKey(KeyCode.A)) return;
        if (Input.GetKey(KeyCode.D)) return;
        if (Input.GetKey(KeyCode.Q)) return;
        if (Input.GetKey(KeyCode.E)) return;
        if (Input.GetKey(KeyCode.S)) return;
        if (Input.GetKey(KeyCode.W)) return;        
        cameraSpeed = 1.0f;
    }
}
