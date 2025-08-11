using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
