using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

/* // keep pos1t1on
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
} // end */

//controller
/*
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

    [Tooltip("Current (applied) angle in degrees")]
    public float maxAngle;

    [Tooltip("Current (applied) angle in degrees")]
    public float mnAngle;

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
        currentAngle = Math.Clamp(currentAngle, mnAngle,maxAngle);
        //currentAngle = Quaternion.RotateTowards(transform.forward, steeringWheel.forward, maxDegreesPerSecond * Time.deltaTime);

        switch (rotationAxis)
        {
            case Axis.X: wheel.localRotation = Quaternion.Euler(25f, 0f, 0f); break;
            case Axis.Y: wheel.localRotation = Quaternion.Euler(25f, currentAngle, 0f); break;
            case Axis.Z: wheel.localRotation = Quaternion.Euler(25f, 0f, currentAngle); break;
        }
    }
} */


// keep pos1t1on

// keep pos1t1on

/*
public class FixChildPosition : MonoBehaviour
{
    private Vector3 initialWorldPos;

    void Start()
    {
        transform.position = new Vector3(0.002f, -0.6169f, 0.0534f);
       
    {
            transform.position = new Vector3(0.002f, -0.6169f, 0.0534f);
        }
    } // end
*/

    public class WheelRotator : MonoBehaviour
    {
        public enum Axis { X, Y, Z }

        [Header("Wheel target to rotate")]
        public Transform wheel;

        [Header("Rotation axis (local)")]
        public Axis rotationAxis = Axis.X;

        [Header("Smoothing (deg/sec). 0 = snap instantly")]
        public float maxDegreesPerSecond = 0f;

        [Tooltip("Raw data received")]
        public float raw;

        [Tooltip("Angle offset added after server value (deg)")]
        public float angleOffset = 0f;

        [Tooltip("Current (applied) angle in degrees")]
        public float currentAngle;

        [Tooltip("Latest commanded angle in degrees (set by client)")]
        public float targetAngle;

        [Tooltip("Current (applied) angle in degrees")]
        public float maxAngle;

        [Tooltip("Current (applied) angle in degrees")]
        public float mnAngle;


    void Reset() { wheel = transform; }

    private void Start()
    {
        GameObject[] clients = GameObject.FindGameObjectsWithTag("Client");
        //GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (clients.Length > 0)
            clients[0].GetComponent<GetFromServer>().wheel = this;
        //players[0].GetComponent<CubeCarCameraGuide>().wheelRotator = this;
        //print("funfou");
    }
    void Update()
        {
        targetAngle = (raw * -1);

        if (!wheel) return;
            float tgt = targetAngle + angleOffset;

            if (maxDegreesPerSecond <= 0f)
                currentAngle = tgt;
            else
                currentAngle = Mathf.MoveTowards(currentAngle, tgt, maxDegreesPerSecond * Time.deltaTime);
            //currentAngle = Quaternion.RotateTowards(transform.forward, steeringWheel.forward, maxDegreesPerSecond * Time.deltaTime);

            switch (rotationAxis)
            {
                case Axis.X: wheel.localRotation = Quaternion.Euler(25f, 0f, 0f); break;
                case Axis.Y: wheel.localRotation = Quaternion.Euler(25f, currentAngle, 0f); break;
                case Axis.Z: wheel.localRotation = Quaternion.Euler(25f, 0f, currentAngle); break;
            }
            Debug.Log("[Steerng Wheel] currentAngle: " + currentAngle.ToString() +
                ", localRotaton: " + wheel.localRotation.ToString());
    }
}


/*
public class FixChildRotation : MonoBehaviour
{
    // Specify the X-axis angle you want to keep (in degrees, local or world depending on need)
    [SerializeField]
    private float fixedXAngle = 0f;

    void LateUpdate()
    {
        Vector3 currentEuler = transform.rotation.eulerAngles;
        // Set only the X component, zero out Y and Z
        transform.rotation = Quaternion.Euler(fixedXAngle, 0f, 0f);
    }
} */
