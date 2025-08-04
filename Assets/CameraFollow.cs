using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 initalOffset;
    private Vector3 cameraPosition;

    public Transform C;

    public Transform C1;

    void Start()
    {
        initalOffset = new Vector3(0, 10, 0);
        transform.position = (C1.position - C.position) / 2;

        transform.position = C.position + transform.position + initalOffset;
        transform.rotation = Quaternion.Euler(90,0,0);

    }

    void FixedUpdate()
    {
        transform.position = (C1.position - C.position) / 2;
        transform.position = C.position + transform.position + initalOffset;
        cameraPosition = transform.position;
    }
}