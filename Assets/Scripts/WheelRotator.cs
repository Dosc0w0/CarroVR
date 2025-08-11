using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// keep pos1t1on
public class FixChildPosition : MonoBehaviour
{
    private Vector3 initialWorldPos;

    void Start()
    {
        transform.position = new Vector3(0.002f, -0.6169f, 0.0534f);
       
    }

    void LateUpdate()
    {
      transform.position = new Vector3(0.002f, -0.6169f, 0.0534f);
    }
} // end

public class WheelRotator : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Wheel target to rotate")]
    public Transform wheel;

    [Header("Rotation axis (local)")]
    public Axis rotationAxis = Axis.X;

    [Header("Smoothing (deg/sec). 0 = snap instantly")]
    public float maxDegreesPerSecond = 0f;

    [Tooltip("Angle offset added after server value (deg)")]
    public float angleOffset = 0f;

    [Tooltip("Current (applied) angle in degrees")]
    public float currentAngle;

    [Tooltip("Latest commanded angle in degrees (set by client)")]
    public float targetAngle;

    void Reset() { wheel = transform; }

    private void Start()
    {
        GameObject[] clients = GameObject.FindGameObjectsWithTag("Client");

        if (clients.Length > 0)
            clients[0].GetComponent<GetFromServer>().wheel = this;
        print("funfou");
    }

    void Update()
    {
        if (!wheel) return;
        float tgt = targetAngle + angleOffset;

        if (maxDegreesPerSecond <= 0f)
            currentAngle = tgt;
        else
            currentAngle = Mathf.MoveTowards(currentAngle, tgt, maxDegreesPerSecond * Time.deltaTime);
        //currentAngle = Quaternion.RotateTowards(transform.forward, steeringWheel.forward, maxDegreesPerSecond * Time.deltaTime);

        switch (rotationAxis)
        {
            case Axis.X: wheel.localRotation = Quaternion.Euler(currentAngle, 0f, 0f); break;
            case Axis.Y: wheel.localRotation = Quaternion.Euler(0f, currentAngle, 0f); break;
            case Axis.Z: wheel.localRotation = Quaternion.Euler(0f, 0f, currentAngle); break;
        }
    }
}
