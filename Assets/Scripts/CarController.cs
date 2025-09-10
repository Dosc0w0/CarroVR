using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CubeCarCameraGuide : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 5f;
    public float maxSpeed = 10f;
    public float deceleration = 4f;

    [Header("Turn Settings")]
    public float turnAcceleration = 90f;
    public float maxTurnSpeed = 120f;
    public float turnDeceleration = 90f;

    [Header("Bounce Settings")]
    public float bounceFactor = 0.5f;
    public float groundY = 0f;

    [Header("Guiding Axis (initialized once)")]
    public Transform guideAxis;

    [Header("(Clent varables)")]
    public WheelRotator wheelRotator;
    public PedalMover pedalAcc;
    public PedalMover pedalBrk;


    private float currentSpeed = 0f;
    private float currentTurnSpeed = 0f;
    private bool cameraAligned = false;
    private bool initialized = false;

    void TryInitialize()
    {
        GameObject[] wheelRotatorObjs = GameObject.FindGameObjectsWithTag("SteeringWheel");
        GameObject[] pedalAccObjs = GameObject.FindGameObjectsWithTag("AccPedal");
        GameObject[] pedalBrkObjs = GameObject.FindGameObjectsWithTag("BrkPedal");

        if (wheelRotatorObjs.Length > 0)
        {
            wheelRotator = wheelRotatorObjs[0].GetComponent<WheelRotator>();
        }
        if (pedalAccObjs.Length > 0)
        {
            pedalAcc = pedalAccObjs[0].GetComponent<PedalMover>();
        }
        if (pedalBrkObjs.Length > 0)
        {
            pedalBrk = pedalBrkObjs[0].GetComponent<PedalMover>();
        }
            
        initialized = wheelRotator != null && pedalAcc != null && pedalBrk != null;
    }

    void Update()
    {
        //Try to initialize the components
        if (!initialized)
        {
            TryInitialize();
        }

        // if was not able to initialize, return
        if (!initialized)
        {
            return;
        }

        // Once guideAxis becomes available, align the camera to it—only once
        if (!cameraAligned && guideAxis != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.rotation = guideAxis.rotation;
                cameraAligned = true;
            }
        }

        // Car controller
        float moveInput = 0;
        if (pedalAcc.currentAngle >= (pedalAcc.maxAngle -  pedalAcc.minAngle) / 2)
        {
            moveInput = math.remap(pedalAcc.minAngle, pedalAcc.maxAngle,
                                        0, 1, pedalAcc.currentAngle); ;
        }

        // Car Controller
         float turnInput = math.remap(wheelRotator.mnAngle,wheelRotator.maxAngle,
                                        1,-1,wheelRotator.currentAngle); 
     
        print("moveInput " + moveInput.ToString() + ", mnAngle " +
            pedalAcc.minAngle.ToString() + ", maxAngle: " + pedalAcc.maxAngle.ToString()
            + ",     currentAngle: " + pedalAcc.currentAngle.ToString());
        // Movement without inertia
        transform.position += transform.forward * moveInput * maxSpeed * Time.deltaTime; // Direct movement
        transform.Rotate(0f, turnInput * maxTurnSpeed * Time.deltaTime, 0f, Space.Self); // Direct turning

        // Speed inertia logic
        /*if (moveInput != 0f)
            currentSpeed += moveInput * acceleration * Time.deltaTime;
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);*/

       // Steering inertia logic
       /* if (turnInput != 0f)
            currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime; 
        else
            currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, 0f, turnDeceleration * Time.deltaTime);
        currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -maxTurnSpeed, maxTurnSpeed); */

        // Apply rotation (steering), independent of guide
        transform.Rotate(0f, currentTurnSpeed * Time.deltaTime, 0f, Space.Self);

        // Movement—guiding axis used only if it was set to initialize camera
        Vector3 movementDir = transform.forward; // movement no longer depends on guide
                                                 //transform.position += movementDir * currentSpeed * Time.deltaTime;


        // Bounce logic
        Vector3 pos = transform.position;
        if (pos.y < groundY)
        {
            pos.y = groundY;
            currentSpeed = bounceFactor;
            currentTurnSpeed *= bounceFactor;
            transform.position = pos;
        }
    }
}
