using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;


public class CubeCarCameraGuide : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float groundY = 0f;

    [Header("Guiding Axis (initialized once)")]
    public Transform guideAxis;

    [Header("(Clent varables)")]
    public WheelRotator wheelRotator;
    public PedalMover pedalAcc;
    public PedalMover pedalBrk;

    // Variaveis controladas por VelocityController
    public float mySpeed = 0f;
    public float speed_world = 0.0f;
    public float acceleration_world = 0.0f;

    // Variaveis controladas por YawController
    public float yaw_rate = 0.0f;
    public float yaw_rate_world = 0.0f;

    private bool cameraAligned = false;
    private bool initialized = false;

    // UI
    public Text velocityText;      // UI Text to show velocity
    public Text accelerationText;  // UI Text to show acceleration
    public Text STAngleText;

    public Transform Rotator;     // will be found at runtime

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

    // Função de atualizar velocidade do carro e parametros pro velocimetro
    public void setInstantSpeed(float speed, float sw, float aw)
    {
        mySpeed = speed;
        speed_world = sw;
        acceleration_world = aw;
    }

    // Função de atualizar o angulo/s de variação do cenario do carro e parametros pro velocimetro
    public void setYawDifference(float yr, float yrw)
    {
        yaw_rate = yr;
        yaw_rate_world = yrw;
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

        // Aplica a velocidade instatanea do carro     
        transform.position += transform.forward * mySpeed;

        // Aplica a variação do yaw (cenario) instataneo do carro
        transform.Rotate(0f, yaw_rate, 0f, Space.Self);

        // Movement—guiding axis used only if it was set to initialize camera
        Vector3 movementDir = transform.forward;

        // Bounce logic
        Vector3 pos = transform.position;
        if (pos.y < groundY)
        {
            pos.y = groundY;
            transform.position = pos;
        }

        // Update UI
        if (velocityText != null)
            velocityText.text = "Speed: " + Mathf.Abs(speed_world).ToString("F0") + " km/h";
        if (accelerationText != null)
            accelerationText.text = "Acc: " + acceleration_world.ToString("F1") + " km/h²";
        if (STAngleText != null)
            STAngleText.text = "Steering: " + yaw_rate_world.ToString("F0") + "°";
    }
}
