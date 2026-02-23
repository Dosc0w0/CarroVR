using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.PlayerSettings;
/*
public class PedalMover : MonoBehaviour
{
    [Header("target to move")]
    public Transform pedal;

    [Header("Smoothing (meter/sec). 0 = snap instantly")]
    public float maxMetersPerSecond = 0f;

    [Tooltip("Current (applied) pos in meters")]
    public float currentZ;

    [Tooltip("Latest commanded poa in meters (set by client)")]
    public float targetZ;

    public float maxZ;

    float startZ;

    private void Start()
    {
        startZ = transform.localPosition.z;
    }

    void Reset() { pedal = transform; }
    void Update()
    {
        if (!pedal) return;

        targetZ = Mathf.Max(startZ, targetZ);
        targetZ = Mathf.Min(maxZ, targetZ);
        float tgt = targetZ;

        if (maxMetersPerSecond <= 0f)
            currentZ = tgt;
        else
            currentZ = Mathf.MoveTowards(currentZ, tgt, maxMetersPerSecond * Time.deltaTime);
        //currentAngle = Quaternion.RotateTowards(transform.forward, steeringWheel.forward, maxDegreesPerSecond * Time.deltaTime);


        pedal.localPosition = new Vector3(pedal.localPosition.x, pedal.localPosition.y, currentZ);
    }
}
*/

public class PedalMover : MonoBehaviour
{
    public string pedalName;

    [Header("Target to rotate")]
    public Transform pedal;

    [Header("Rotation speed (degrees/sec). 0 = snap instantly")]
    public float maxDegreesPerSecond = 0f;

    [Tooltip("Raw data received")]
    public float raw;

    [Tooltip("Current applied rotation (degrees)")]
    public float currentAngle;

    [Tooltip("Latest commanded rotation (degrees, set by client)")]
    public float targetAngle;

    public float minAngle; // e.g., 0
    public float maxAngle; // e.g., 30

    private float startAngle;

    private void Start()
    {
        startAngle = pedal.localEulerAngles.x; // Assuming pedal rotates on X-axis

        // Set pedal on Client
        GameObject[] clients = GameObject.FindGameObjectsWithTag("Client");

        if (clients.Length > 0)
        {
            if (pedalName == "Acc")
            {
                clients[0].GetComponent<GetFromServer>().pedalAcc = this;
            }
            else if (pedalName == "Brk")
            {
                clients[0].GetComponent<GetFromServer>().pedalFreio = this;
            }
        }

        //print("funfou");
    }

    void Reset() { pedal = transform; }

    void Update()
    {
        if (!pedal) return;
        targetAngle = (raw / 10.0f);

        // Clamp target rotation
        targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);

        // Smooth movement
        if (maxDegreesPerSecond <= 0f)
            currentAngle = targetAngle;
        else
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, maxDegreesPerSecond * Time.deltaTime);

        // Apply rotation on the local X-axis
        pedal.localRotation = Quaternion.Euler(startAngle+currentAngle, pedal.localEulerAngles.y, pedal.localEulerAngles.z);
    }
}
