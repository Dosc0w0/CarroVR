/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeCarController : MonoBehaviour
{
    public float acceleration = 10f;
    public float maxSpeed = 20f;
    public float turnSpeed = 100f;
    private Rigidbody rb;
    private float inputX, inputY;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Optional: lower center of mass for stability
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        // Gather input each frame
        inputY = Input.GetAxis("Vertical");
        inputX = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        // Apply forward/backward force
        Vector3 force = transform.forward * inputY * acceleration;
        rb.AddForce(force, ForceMode.Acceleration);

        // Apply rotation (only when the car moves forward/backward)
        if (Mathf.Abs(rb.velocity.magnitude) > 0.1f)
        {
            float turn = inputX * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0, turn, 0);
            rb.MoveRotation(rb.rotation * turnRotation);
        }

        // Cap max speed
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (flatVel.magnitude > maxSpeed)
        {
            rb.velocity = flatVel.normalized * maxSpeed + Vector3.up * rb.velocity.y;
        }
    }
}
*/

// - - - - - - - -

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCarController_NoRigidbody : MonoBehaviour
{
    public float speed = 10f;           // Forward/Reverse speed
    public float turnSpeed = 50f;      // Turning speed (degrees per second)
    private float moveInput;
    private float turnInput;

    void Update()
    {
        // Get player input
        moveInput = Input.GetAxis("Vertical");     // W/S or Up/Down arrows
        turnInput = Input.GetAxis("Horizontal");   // A/D or Left/Right arrows

        // Move forward or backward
        Vector3 move = transform.forward * moveInput * speed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // Rotate the object
        float turnAngle = turnInput * turnSpeed * Time.deltaTime;
        transform.Rotate(0f, turnAngle, 0f, Space.World);
    }
}
*/

// - - - - - - - -

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCarPhysics_NoRigidbody : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 5f;       // Rate of speed change (units/s²)
    public float maxSpeed = 10f;          // Cap on forward/backward speed
    public float deceleration = 4f;       // Rate of slowing when no input

    [Header("Rotation Settings")]
    public float turnAcceleration = 90f;  // Rate of steering speed change (°/s²)
    public float maxTurnSpeed = 120f;     // Max steering speed (°/s)
    public float turnDeceleration = 90f;  // Steering slowdown (°/s²)

    [Header("Bounce Settings")]
    public float bounceFactor = 0.5f;     // Bounce strength (0-1)
    public float groundY = 0f;            // Ground plane Y-level

    private float currentSpeed = 0f;
    private float currentTurnSpeed = 0f;

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Vertical");   // Forward/back input
        float turnInput = Input.GetAxisRaw("Horizontal"); // Steering input

        //–– Handle acceleration and deceleration ––
        if (moveInput != 0f) {
            currentSpeed += moveInput * acceleration * Time.deltaTime;
        } else {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        //–– Handle steering inertia ––
        if (turnInput != 0f) {
            currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime;
        } else {
            currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, 0f, turnDeceleration * Time.deltaTime);
        }
        currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

        //–– Apply rotation (steering) and forward movement ––
        transform.Rotate(0f, currentTurnSpeed * Time.deltaTime, 0f, Space.World);
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);

        //–– Simulate bouncing if cube hits ground ––
        Vector3 pos = transform.position;
        if (pos.y < groundY) {
            pos.y = groundY;
            currentSpeed *= bounceFactor;        // reduce speed upon bounce
            currentTurnSpeed *= bounceFactor;    // also damp rotation
            transform.position = pos;
        }
    }
} 
*/

/*
using System.Collections;
using System.Collections.Generic;
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

    [Header("Guiding Axis")]
    public Transform guideAxis;

    private float currentSpeed = 0f;
    private float currentTurnSpeed = 0f;

    void Start()
    {
        // Attach the watcher to guideAxis if provided
        if (guideAxis != null)
        {
            var watcher = guideAxis.gameObject.AddComponent<GuideWatcher>();
            watcher.parentController = this;
        }
    }

    // Called from GuideWatcher when guide “arises”
    public void AlignCameraToGuide()
    {
        Camera cam = Camera.main;
        if (cam != null && guideAxis != null)
        {
            cam.transform.rotation = guideAxis.rotation;
            Debug.Log("Camera aligned to guide at reactivation.");
        }
    }

    void Update()
    {
        // [ Existing movement + bounce logic unchanged ]

        float moveInput = Input.GetAxisRaw("Vertical");
        float turnInput = Input.GetAxisRaw("Horizontal");

        // Speed inertia
        currentSpeed = (moveInput != 0f)
            ? currentSpeed + moveInput * acceleration * Time.deltaTime
            : Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // Turn inertia
        currentTurnSpeed = (turnInput != 0f)
            ? currentTurnSpeed + turnInput * turnAcceleration * Time.deltaTime
            : Mathf.MoveTowards(currentTurnSpeed, 0f, turnDeceleration * Time.deltaTime);
        currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

        // Apply steering & movement
        transform.Rotate(0f, currentTurnSpeed * Time.deltaTime, 0f, Space.Self);
        transform.position += transform.forward * currentSpeed * Time.deltaTime;

        // Bounce at ground level
        Vector3 pos = transform.position;
        if (pos.y < groundY)
        {
            pos.y = groundY;
            currentSpeed *= bounceFactor;
            currentTurnSpeed *= bounceFactor;
            transform.position = pos;
        }
    }
}
*/


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
    WheelRotator wheelRotator;
    PedalMover pedalAcc;
    PedalMover pedalBrk;


    private float currentSpeed = 0f;
    private float currentTurnSpeed = 0f;
    private bool cameraAligned = false;

    void Update()
    {
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

        // Quest 3 controller Handle movement input
        //float moveInput = Input.GetAxisRaw("Vertical");

        // Car controller
        float moveInput = 0;
        if (pedalAcc.currentAngle >= (pedalAcc.maxAngle -  pedalAcc.minAngle) / 2)
        {
            moveInput = math.remap(pedalAcc.minAngle, pedalAcc.maxAngle,
                                        0, 1, pedalAcc.currentAngle); ;
        }
        else if (pedalBrk.currentAngle >= (pedalBrk.maxAngle - pedalBrk.minAngle) / 2)
        {
            moveInput = math.remap(pedalBrk.minAngle, pedalBrk.maxAngle,
                                        0, -1, pedalBrk.currentAngle); ;
        }

        // Quest 3 controller
        //float turnInput = Input.GetAxisRaw("Horizontal");

        // Car Controller
        float turnInput = math.remap(wheelRotator.mnAngle,wheelRotator.maxAngle,
                                        -1,1,wheelRotator.currentAngle);

        // Speed inertia logic
        if (moveInput != 0f)
            currentSpeed += moveInput * acceleration * Time.deltaTime;
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // Steering inertia logic
        if (turnInput != 0f)
            currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime;
        else
            currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, 0f, turnDeceleration * Time.deltaTime);
        currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

        // Apply rotation (steering), independent of guide
        transform.Rotate(0f, currentTurnSpeed * Time.deltaTime, 0f, Space.Self);

        // Movement—guiding axis used only if it was set to initialize camera
        Vector3 movementDir = transform.forward; // movement no longer depends on guide
        transform.position += movementDir * currentSpeed * Time.deltaTime;

        // Bounce logic
        Vector3 pos = transform.position;
        if (pos.y < groundY)
        {
            pos.y = groundY;
            currentSpeed *= bounceFactor;
            currentTurnSpeed *= bounceFactor;
            transform.position = pos;
        }
    }
}

/*
using System.Collections;
using System.Collections.Generic;
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

    private float currentSpeed = 0f;
    private float currentTurnSpeed = 0f;
    private bool cameraAligned = false;

    void Update()
    {
        // Align camera once
        if (!cameraAligned && guideAxis != null)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.rotation = guideAxis.rotation;
                cameraAligned = true;
            }
        }

        // Read input from Quest 3 right-hand controller:
        bool aPressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);      // A - rotate right
        bool bPressed = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch);      // B - rotate left

        float triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        bool triggerPressed = triggerValue > 0.5f;  // Move forward when pressed beyond half-press

        // Convert to input values:
        float moveInput = triggerPressed ? 1f : 0f;
        float turnInput = (aPressed ? 1f : 0f) + (bPressed ? -1f : 0f);

        // Handle movement inertia
        if (moveInput != 0f)
            currentSpeed += moveInput * acceleration * Time.deltaTime;
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        // Handle turning inertia
        if (turnInput != 0f)
            currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime;
        else
            currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, 0f, turnDeceleration * Time.deltaTime);

        currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

        // Apply rotation and movement
        transform.Rotate(0f, currentTurnSpeed * Time.deltaTime, 0f, Space.Self);
        transform.position += transform.forward * currentSpeed * Time.deltaTime;

        // Bounce logic
        Vector3 pos = transform.position;
        if (pos.y < groundY)
        {
            pos.y = groundY;
            currentSpeed *= bounceFactor;
            currentTurnSpeed *= bounceFactor;
            transform.position = pos;
        }
    }
}
*/
